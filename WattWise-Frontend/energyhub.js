const connection = new signalR.HubConnectionBuilder()
    .withUrl(AuthHelper.API_BASE_URL + "/energyhub", {
        withCredentials: true
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

async function initSignalR() {
    try {
        const isAuthenticated = await AuthHelper.checkAuth();
        if (isAuthenticated) {
            await connection.start();
            console.log("SignalR Connected Successfully via Cookies!");
        } else {
            console.warn("SignalR Connection Aborted: User is not authenticated.");
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
    }
}

initSignalR();
