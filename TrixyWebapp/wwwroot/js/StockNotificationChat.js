var notifiction = [];
console.log("Logged-in User ID:", currentUserId);
$(document).ready(function () {
    if (!currentUserId) return; // Ensure user is logged in

    let storedData = localStorage.getItem(`stockNotifications_${currentUserId}`);
    if (storedData) {
        notifiction = JSON.parse(storedData); 
        displayStockNotifications(notifiction); 
       
    }
});
const connection = new signalR.HubConnectionBuilder()
	.withUrl("/stockNotificationHub")
    .build();
 
connection.on("ReceiveStockUpdate", (stocknotifactiondatas) => {
   
    console.log("Received Data:", stocknotifactiondatas); // Debugging
    if (!Array.isArray(stocknotifactiondatas) || stocknotifactiondatas.length === 0) {
        console.warn("No data received or incorrect format");
        return;
    }
    let userNotifications = stocknotifactiondatas.filter(item => item.userid === currentUserId);
    notifiction = [...userNotifications, ...notifiction].slice(0, 50);
   
    localStorage.setItem(`stockNotifications_${currentUserId}`, JSON.stringify(notifiction));
    
    // Display notifications
    displayStockNotifications(notifiction);
   window.location.reload();
    //console.log("Stock notifications updated!");
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
                ? `<span class="lable lable-danger">Sell</span>`
                : `<span class="lable lable-success">Buy</span>`;
        }
        const signalClass = item.buySellSignal === 'SELL' ? 'label-danger' : 'label-success';
        userDiv.innerHTML = `
            <div class="media-body">
                <p class="fs-16">
                    <strong>Stock:</strong> ${item.symbol} 
                </p>
                <p> <strong>Price:</strong> ${item.price}</p>
                <p><strong>Signal:</strong>
           <span class="label ${signalClass}">${item.buySellSignal}</span></p>
                <p><strong>Strategy used:</strong> ${item.userStrategy.map(strategy => strategy).join(', ')}</p>
                <span>${formattedTime}</span>
            </div>
        `;

        var recommendationCell = $("#recomendation_" + item.symbol);
        if (recommendationCell.length > 0) {
            recommendationCell.html(recommendationLabel);  // Update the table cell
        }
        container.appendChild(userDiv); 
    });
   
    console.log("Stock notifications updated!");
}
function getCurrentUserId() {
   
    // Example: Fetch user ID from a global JS variable or an API
    return sessionStorage.getItem("UserId") || null;
}