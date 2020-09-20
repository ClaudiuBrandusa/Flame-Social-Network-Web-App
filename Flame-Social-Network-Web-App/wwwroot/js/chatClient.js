"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", {
        accessTokenFactory: () => "testing"
    })
    .build();
// Connection events
connection.on("UserConnected", function () {
    connection.invoke("ListContacts");
});

connection.on("UserDisconnected", function () {
    connection.invoke("ListContacts");
});

// Contacts List
connection.on("ListContacts", function (list) {
    var counter = document.getElementById("listCount")

    // we are looking for the contact list
    var ul = document.getElementById("list");
    if (ul == null) {
        return;
    }
    if (list.length == 0) {
        ul.innerHTML = "There are no online contacts in the list";
        return;
    }
    // clearing the list
    ul.innerHTML = "";
    // configuring the list's rows
    for (var i = 0; i < list.length; i++) {
        var row = document.createElement("li");
        row.classList.add("no-list-style");
        var anchor = document.createElement("a");
        anchor.addEventListener("click", function () {
            connection.invoke("ChatWith", anchor.innerText);
        });
        anchor.classList.add("link");
        row.appendChild(anchor);
        anchor.innerHTML = list[i];
        ul.appendChild(row);
    }
});

connection.on("alert", function (text) {
    alert(text);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});