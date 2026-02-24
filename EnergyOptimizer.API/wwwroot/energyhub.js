const connection = new signalR.HubConnectionBuilder()
    .withUrl("/energyhub", {
        accessTokenFactory: () => AuthHelper.getToken() 
    })
    .withAutomaticReconnect()
    .build();

toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-left",
    "timeOut": "5000"
};

connection.on("ReceiveAlert", (alertJson) => {
    const alert = typeof alertJson === 'string' ? JSON.parse(alertJson) : alertJson;
    AuthHelper.broadcastEvent('app-alert', alert);
});

connection.on("DeviceStatusUpdated", (data) => {
    AuthHelper.broadcastEvent('device-status-change', data);
});

connection.on("ReceiveReadings", (readings) => {
    AuthHelper.broadcastEvent('new-readings', readings);
});

async function startSignalR() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(startSignalR, 5000); 
    }
}

if (AuthHelper.isAuthenticated()) {
    startSignalR();
}