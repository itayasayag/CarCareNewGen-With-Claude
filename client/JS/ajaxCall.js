
/*server = `https://proj.ruppin.ac.il/cgroup71/test2/tar2`*/

let port = "7024";
server = `https://localhost:${port}`;

function ajaxCall(method, api, data, successCB, errorCB) {
    $.ajax({
        type: method,
        url: api,
        data: data,
        cache: false,
        contentType: "application/json",
        dataType: "json",
        success: successCB,
        error: errorCB
    });
}