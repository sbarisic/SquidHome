function getByClassInsideID(id, classname) {
	return document.getElementById(id).getElementsByClassName(classname)[0];
}

function ws_refresh_page() {
	location.reload();
}

function ws_set_inner(id, classname, inner) {
	var element = getByClassInsideID(id, classname);

	if (element != null) {
		element.innerHTML = inner;
	}
}

function ws_send_event(event_name, args) {
	if (squid_ws.readyState === WebSocket.OPEN) {
		squid_ws.send(JSON.stringify({ EventName: event_name, Args: args }));
		return false;
	}

	return true;
}

let squid_ws = new WebSocket("ws://127.0.0.1:8081/ws");

squid_ws.onopen = function (e) {
}

squid_ws.onmessage = function (e) {
	var data = JSON.parse(e.data);
	console.log(data);

	if (data.EventName == "ws_set_inner") {
		ws_set_inner(data.ID, data.ClassName, data.Inner);
	} else if (data.EventName == "ws_refresh_page") {
		ws_refresh_page();
	}
}