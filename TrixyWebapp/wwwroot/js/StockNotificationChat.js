
const connection = new signalR.HubConnectionBuilder()
	.withUrl("/stockNotificationHub")
    .build();
    debugger
connection.on("ReceiveStockUpdate", (stocknotifactiondatas) => {
    debugger
    console.log("Received Data:", stocknotifactiondatas); // Debugging
    if (!Array.isArray(stocknotifactiondatas) || stocknotifactiondatas.length === 0) {
        console.warn("No data received or incorrect format");
        return;
    }

    let storedData = localStorage.getItem("stockNotifications");
    let existingData = storedData ? JSON.parse(storedData) : [];

    // Merge old and new notifications (limit to last 50 notifications for performance)
    let updatedStockNotifications = [...stocknotifactiondatas, ...existingData];

    localStorage.setItem("stockNotifications", JSON.stringify(stocknotifactiondatas));

    // Display notifications
    displayStockNotifications(updatedStockNotifications);
    //let container = document.getElementById("notificationContainer"); // Target div
    //if (!container) {
    //    console.error("Target div (notificationContainer) not found");
    //    return;
    //}

    //stocknotifactiondatas.forEach(item => {
    //    let userDiv = document.createElement("div");
    //    userDiv.classList.add("media", "py-10", "px-0");
    //    let formattedTime = timeAgo(item.timestamp);

    //    if (item.buySellSignal) {
    //        if (item.buySellSignal.toUpperCase() === "SELL") {
    //            recommendationLabel = `<span class="label label-danger">Sell</span>`;
    //        } else {
    //            recommendationLabel = `<span class="label label-success">Buy</span>`;
    //        }
    //    }

    //    userDiv.innerHTML = `
    //        <a class="avatar avatar-lg status-danger" href="#">
    //            <img src="/assets/images/1.jpg" alt="...">
    //        </a>
    //        <div class="media-body">
    //            <p class="fs-16">
    //                <a class="hover-primary" href="#"><strong>${item.userid}</strong></a>
    //            </p>
    //            <p><strong>Stock:</strong>  ${item.symbol} - <strong>Price:</strong>  ${item.price} - Signal: ${item.buySellSignal}</p>
    //            <p><strong>Strategy used:</strong>  ${item.userStrategy.map(strategy => strategy).join(', ') } </p>
    //            <span>${formattedTime}</span>
    //        </div>
    //    `;
    //    var recommendationCell = $("#recomendation_" + item.symbol);
    //    if (recommendationCell.length > 0) {
    //        recommendationCell.html(recommendationLabel);  // Update the table cell
    //    }
    //    container.appendChild(userDiv); // Append new stock update
    //});
    console.log("Stock notifications updated!");
});

connection.start()
    .then(() => console.log("Connected to Hub with Connection ID:", connection.connectionId))
	.catch(err => console.error(err));

function requestOnlineUsers() {
	connection.invoke("GetOnlineUsers");
}
function timeAgo(timestamp) {
    const now = new Date();
    const past = new Date(timestamp);
    const diffInSeconds = Math.floor((now - past) / 1000);

    if (diffInSeconds < 60) return "Just now";
    const diffInMinutes = Math.floor(diffInSeconds / 60);
    if (diffInMinutes < 60) return `${diffInMinutes} min ago`;
    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours} hours ago`;
    const diffInDays = Math.floor(diffInHours / 24);
    return `${diffInDays} days ago`;
}

// Function to display stock notifications
function displayStockNotifications(stocknotifactiondatas) {
    let container = document.getElementById("notificationContainer");
    if (!container) {
        console.error("Target div (notificationContainer) not found");
        return;
    }

    container.innerHTML = ""; // Clear existing notifications

    stocknotifactiondatas.forEach(item => {
        let userDiv = document.createElement("div");
        userDiv.classList.add("media", "py-10", "px-0");
        let formattedTime = timeAgo(item.timestamp);

        let recommendationLabel = "";
        if (item.buySellSignal) {
            recommendationLabel = item.buySellSignal.toUpperCase() === "SELL"
                ? `<span class="label label-danger">Sell</span>`
                : `<span class="label label-success">Buy</span>`;
        }

        userDiv.innerHTML = `
            <a class="avatar avatar-lg status-danger" href="#">
                <img src="/assets/images/1.jpg" alt="...">
            </a>
            <div class="media-body">
                <p class="fs-16">
                    <a class="hover-primary" href="#"><strong>${item.userid}</strong></a>
                </p>
                <p><strong>Stock:</strong> ${item.symbol} - <strong>Price:</strong> ${item.price} - Signal: ${item.buySellSignal}</p>
                <p><strong>Strategy used:</strong> ${item.userStrategy.map(strategy => strategy).join(', ')}</p>
                <span>${formattedTime}</span>
            </div>
        `;

        var recommendationCell = $("#recomendation_" + item.symbol);
        if (recommendationCell.length > 0) {
            recommendationCell.html(recommendationLabel);  // Update the table cell
        }
        container.appendChild(userDiv); // Append new stock update
    });

    console.log("Stock notifications updated!");
}
document.addEventListener("DOMContentLoaded", () => {
    let storedData = localStorage.getItem("stockNotifications");
    if (storedData) {
        let stocknotifactiondatas = JSON.parse(storedData);
        displayStockNotifications(stocknotifactiondatas);
    }
});