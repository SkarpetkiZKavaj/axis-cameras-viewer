async function addCamera() {
    var url = document.getElementById('camera-url').value;
    if (url) {
        let cameraId = await createCamera(url);
        
        renderCamera(cameraId, url);
        connectToCameraHub();
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

function connectToCameraHub() {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/cameraHub")
        .build();

    connection.on('SendImage', (cameraId, image) => {
        let img = document.getElementById(cameraId);
        img.src = `data:image/jpeg;base64,${image}`;
    });

    connection.start().catch(err => console.error(err.toString()));
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