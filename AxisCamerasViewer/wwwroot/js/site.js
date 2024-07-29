async function addCamera() {
    var url = document.getElementById('camera-url').value;
    if (url) {
        let cameraId = await createCamera(url);
        
        renderCamera(cameraId, url);
    }
}

function renderCamera(cameraId, url) {
    let img = document.createElement('img');
    img.src = `/viewer/camera/${cameraId}`;
    img.id = cameraId;

    let cameraLink = document.createElement('a');
    cameraLink.innerText = url;
    cameraLink.href = url;

    let closeButton = document.createElement('button');
    closeButton.innerText = 'Close';
    closeButton.onclick = () => removeCamera(cameraId);

    let bottomContainer = document.createElement('div');
    bottomContainer.appendChild(cameraLink);
    bottomContainer.appendChild(closeButton);
    bottomContainer.className = 'bottom-container';

    let streamContainer = document.createElement('div');
    streamContainer.appendChild(img);
    streamContainer.appendChild(bottomContainer);
    streamContainer.className = 'stream-container';
    streamContainer.id = `container-${cameraId}`;

    document.getElementById('streams').appendChild(streamContainer);
}

async function createCamera(url) {
    const response = await fetch('/viewer/camera', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ url })
    });
    
    var { id } = await response.json();
    
    return id;
}

async function removeCamera(id) {
    await fetch(`/viewer/camera/${id}`, { method: 'DELETE' });
    
    var streamContainer = document.getElementById(`container-${id}`);
    streamContainer.remove();
}