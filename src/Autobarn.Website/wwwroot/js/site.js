// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(connectToSignalR);

function displayNotification(user, message) {
    var $target = $("div#signalr-notifications");
    var data = JSON.parse(message);
    var message =
        `NEW VEHICLE! <a href="/vehicles/details/${data.registration}">${data.registration}</a> (${data.manufacturer} ${
            data.modelName}, 
        ${data.color}, ${data.year}). 
        Price ${data.currencyCode} ${data.price}`;
    var $div = $(`<div>${message}</div>`);
    $target.prepend($div);
    window.setTimeout(function() { $div.fadeOut(2000, function() { $div.remove(); }); }, 8000);
}

function connectToSignalR() {
    const conn = new signalR.HubConnectionBuilder().withUrl("/hub").build();
    conn.on("DisplayNotification", displayNotification);
    conn.start().then(function () {
        console.log("SignalR started!");
    }).catch(function (err) {
        console.log(err);
    });
}