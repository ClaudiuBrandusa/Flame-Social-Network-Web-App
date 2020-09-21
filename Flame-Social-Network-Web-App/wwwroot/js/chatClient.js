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
    if (counter == null) {
        return; // then it means that we can't chat here
    }
    counter.innerHTML = list.length;

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
        anchor.style.cursor = "pointer";
        row.appendChild(anchor);
        row.classList.add("text-overflow-hide");
        anchor.innerText = list[i];
        ul.appendChild(row);
    }
});

connection.on("openChatRoomWith", function (receiver, chatroom_id) {
    //var username = document.getElementById("Name").value;
    /*Create chatroom front end*/
    var space = document.getElementById("chat-space");
    if (space == null) {
        return; // then we can't open a chat room
    }
    // we check if we have already the chat room open
    if (document.getElementById("room_id_" + chatroom_id) != null) {
        return;
    }
    var room = document.createElement("div");
    room.classList.add("chat-room");
    space.insertBefore(room, space.firstChild);
    // top bar part
    var top_bar = document.createElement("div");
    top_bar.classList.add("top-bar");
    room.id = "room_id_" + chatroom_id;
    var title = document.createElement("div");
    title.classList.add("chat-title");
    title.classList.add("pl-1");
    top_bar.appendChild(title);
    room.appendChild(top_bar);
    // title should point to the receiver's page
    var img = document.createElement("img");
    img.classList.add("recipient-avatar-img");
    img.src = location.protocol + "//" + location.host + '/images/avatar_icon.png'; // default img for now
    title.appendChild(img);
    var anchor = document.createElement("a");
    anchor.classList.add("pl-2");
    anchor.classList.add("no-select");
    // here we can add a link to the user's profile
    anchor.innerText = receiver
    title.appendChild(anchor);
    var exit = document.createElement("div");
    exit.classList.add("chat-exit");
    exit.classList.add("text-center");
    exit.innerText = "X";
    exit.addEventListener("click", function () {
        connection.invoke("EndChat", chatroom_id);
    });
    top_bar.appendChild(exit);

    // chat history part
    var chat_history = document.createElement("div");
    chat_history.classList.add("chat-history");
    var chat_content = document.createElement("div");
    chat_content.classList.add("pl-1");
    chat_content.classList.add("pr-1");
    chat_content.id = "chat_content_" + room.id;
    chat_history.appendChild(chat_content);
    room.appendChild(chat_history);

    // control bar part
    var control_bar = document.createElement("div");
    control_bar.classList.add("control-bar");
    var input_message = document.createElement("div");
    input_message.classList.add("input-message");
    var input = document.createElement("input");
    input.id = "input_" + chatroom_id;
    input.name = "input-message";
    input.classList.add("h-100");
    input_message.appendChild(input);
    control_bar.appendChild(input_message);
    var send_message = document.createElement("div");
    send_message.classList.add("send-message");
    var submit_button = document.createElement("button");
    submit_button.type = "submit";
    submit_button.classList.add("h-100");
    submit_button.classList.add("button-style");
    submit_button.onclick = function () {
        // firstly we check if we have empty input
        var element = document.getElementById("input_" + chatroom_id);
        var msg = element.value;
        if (msg.length == 0) {
            return;
        }
        // then we send the message to the server
        connection.invoke("SendMessageTo", msg, chatroom_id);
        // we clear the input area
        element.value = "";
    };
    // keydown events
    input.addEventListener("keydown", function (event) {
        if (event.keyCode === 13 && event.shiftKey) {
            // here are adding a new line
            //input.value += ;
        } else if (event.keyCode === 13) {
            submit_button.click();
        }
    });
    submit_button.innerText = "Send";
    send_message.appendChild(submit_button);
    control_bar.appendChild(send_message);
    room.appendChild(control_bar);
});

connection.on("SendMessageIn", function (chatroom_id, message) {
    var space = document.getElementById("chat-space");
    if (space == null) {
        return; // then we can't get the chat room
    }
    if (document.getElementById("room_id_" + chatroom_id) == null) {
        // something went wrong
        return;
    }
    var chat_content = document.getElementById("chat_content_room_id_" + chatroom_id);
    if (chat_content == null) {
        // then something went wrong
        return;
    }
    var text_space = document.createElement("div");
    text_space.classList.add("sent-text-space");
    var text = document.createElement("div");
    text.classList.add("sent-text");
    text.innerText = message;
    text_space.appendChild(text);
    chat_content.appendChild(text_space);
});

connection.on("ReceiveMessageIn", function (chatroom_id, message, tag) {
    var space = document.getElementById("chat-space");
    if (space == null) {
        return; // then we can't get the chat room
    }
    if (document.getElementById("room_id_" + chatroom_id) == null) {
        // then something went wrong
        return;
    }
    var chat_content = document.getElementById("chat_content_room_id_" + chatroom_id);
    if (chat_content == null) {
        // then something went wrong
        return;
    }
    var text_space = document.createElement("div");
    text_space.classList.add("received-text-space");
    var img = document.createElement("img");
    img.src = location.protocol + "//" + location.host + "/images/avatar_icon.png";
    img.classList.add("recipient-avatar-img");
    text_space.appendChild(img);
    var text = document.createElement("div");
    text.classList.add("received-text");
    text.innerText = message;
    text_space.appendChild(text);
    chat_content.appendChild(text_space);
});

connection.on("closeChatRoom", function (room_id) {
    var space = document.getElementById("chat-space");
    if (space == null) {
        return; // then we don't have chat rooms
    }
    var room = document.getElementById("room_id_" + room_id);
    if (room == null) {
        return;
    }
    room.parentNode.removeChild(room);
});

connection.on("alert", function (text) {
    alert(text);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});