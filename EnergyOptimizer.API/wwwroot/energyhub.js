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

connection.on("ReceiveAlert", (alert) => {
    const message = `${alert.deviceName}: ${alert.message}`;

    if (alert.severity === 3) {
        toastr.error(message, "Critical Alert!");
    } else if (alert.severity === 2) {
        toastr.warning(message, "Warnning");
    } else {
        toastr.info(message, "Information");
    }
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