// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function changeNecessaryInputs() {
    let transaction_input = document.getElementById('transactions_number');
    let first_date_input = document.getElementById('first-date');
    let last_date_input = document.getElementById('last-date');
    var rad = document.getElementsByName('Method');
    if (rad[1].checked) {
        transaction_input.disabled = false;
        first_date_input.disabled = true;
        last_date_input.disabled = true;
    } else {
        transaction_input.disabled = true;
        first_date_input.disabled = false;
        last_date_input.disabled = false;
    }
}

function Modify(type, key, tableName) {
    array = [tableName];
    let row = document.getElementById(key);
    let columns = row.getElementsByTagName('td');
    let url = '/Data/'+ type +'?items=' + tableName + '&';

    for (var i = 0; i < columns.length - 1; i++) {
        url = url + 'items=' + encodeURIComponent(row.getElementsByTagName("td")[i].innerHTML);
        if (i < columns.length - 2) url = url + '&';
    }
    if (type == 'Delete') {
        let link = "location.href='" + url + "'";
        let button = document.getElementById('deleteButton').setAttribute('onclick', link);
        show('delete');
    } else if (type == 'Edit') {
        document.location.href = url;
    }
}
    
$ = function (id) {
    return document.getElementById(id);
}

var show = function (id) {
    $(id).style.display = 'block';
}
var hide = function (id) {
    $(id).style.display = 'none';
}

function saveToPC(str) {
    let blob = new Blob([str], { type: "text/plain" });
    let link = document.createElement("a");
    link.setAttribute("href", URL.createObjectURL(blob));
    link.setAttribute("download", document.getElementById('fileName').value + ".csv");
    link.click();
    hide('export');
}

