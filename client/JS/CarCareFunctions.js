// Set the default Swal confirm button text to Hebrew "סגור" app-wide.
// Using mixin so individual calls can still override it.
if (typeof Swal !== 'undefined') {
    window.Swal = Swal.mixin({ confirmButtonText: 'סגור' });
}

﻿function AddCarToListFromStorage() {
    var temp = localStorage.getItem('LogInUser');
    var tempUser = JSON.parse(temp);
    if (GetCachedCarList() == null) {
        // No usable cached list — fetch first, then render once data is back.
        ReadUserCarByEmail(tempUser.email, RenderCarSelectFromStorage);
        return;
    }
    // Render immediately from cache, then refresh in the background so
    // newly added/removed cars show up without needing a manual reload.
    RenderCarSelectFromStorage();
    ReadUserCarByEmail(tempUser.email, RenderCarSelectFromStorage);
}

function RenderCarSelectFromStorage() {
    var StorageCarlist = GetCachedCarList();
    var valueCounter = 1;/* משתנה רץ לאופשנס*/
    var str = "<select onchange='changeCarSelect()' class='select-car' id='carSelect' name='carSelect'> <option value='' disabled selected>בחר רכב</option>";
    if (StorageCarlist != null) {
        for (var i = 0; i < StorageCarlist.length; i++) {
            str += `<option value='${StorageCarlist[i].licensePlate}'>` + StorageCarlist[i].nickName + '</option>';
        }
    }
    str += "<option style='background-color: #99BFD2' value='addCar'>הוסף רכב+ </option></select>";
    document.getElementById("SelectDiv").innerHTML = str;

    // Preserve previously selected car in the dropdown, if it still exists in the list.
    var selectedCarValue = JSON.parse(localStorage.getItem('SelectedCar'));
    if (selectedCarValue !== null && StorageCarlist != null &&
        StorageCarlist.some(function (c) { return String(c.licensePlate) === String(selectedCarValue); })) {
        $('#carSelect').val(selectedCarValue);
    }
}


function AddCarToAlertsListFromStorage() { //טיפה שונה מהוספה רגילה בגלל העיצוב הוספה בעמוד התראות
    var temp = localStorage.getItem('LogInUser');
    var StorageCarlist = GetCachedCarList() || [];
    var tempUser = JSON.parse(temp);
    userEmail = tempUser.email;
    var valueCounter = 1;/* משתנה רץ לאופשנס*/
    var str = "<select class='carcare-select' id='carSelect' name='carSelect'> <option value='' disabled selected>בחר רכב</option>";
    for (var i = 0; i < StorageCarlist.length; i++) {
        if (StorageCarlist[i].userEmail == userEmail) {
            str += `<option value='${StorageCarlist[i].licensePlate}'>` + StorageCarlist[i].nickName + '</option>';//TBD מתלבט אם להוסיף ואליו מספר רכב
            valueCounter++;
        }
    }
    str += "</select>";
    document.getElementById("SelectDiv").innerHTML = str;
}


// Safely read the cached car list. Returns null if it's missing or corrupt.
// (A past bug wrote invalid JSON here, which made JSON.parse throw and the
// whole car list silently disappear until a fresh fetch overwrote it.)
function GetCachedCarList() {
    var raw = localStorage.getItem('UserCarlist');
    if (raw == null) return null;
    try {
        var parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed : null;
    } catch (e) {
        console.warn('UserCarlist cache was corrupt — clearing it.', e);
        localStorage.removeItem('UserCarlist');
        return null;
    }
}

function ReadUserCarByEmail(email, onDone)
{
    api = server + "/api/usercars/"+email;
    ajaxCall("GET", api, "", function (suc) { getSCB(suc, onDone); }, getECB);
}

function getSCB(suc, onDone) {
    localStorage.setItem('UserCarlist', JSON.stringify(suc))
    if (typeof onDone === 'function') onDone();
}

function getECB(err) {
    console.log(err);
}

function SaveCarsToLocalStorage(CarList) {
    localStorage.setItem('UserCarlist', JSON.stringify(CarList))
}

//קריאת רשימת טיפולים של משתמש ללוקאל
function ReadUserCarCareByEmail(email, onDone) {
    api = server + "/api/logs/" + email;
    ajaxCall("GET", api, "", function (suc) { getCCSCB(suc, onDone); }, getCCECB);
}

function getCCSCB(suc, onDone) {
    localStorage.setItem('UserCarCarelist', JSON.stringify(suc))
    if (typeof onDone === 'function') onDone();
}

function getCCECB(err) {
    console.log(err);
}


let map, service, infowindow;

function initMap() {
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: 31.0461, lng: 34.8516 }, // Coordinates for Israel
        zoom: 8
    });
    infowindow = new google.maps.InfoWindow();
}

function sendUser() {

}
function searchCity() {
    const city = document.getElementById('cityInput').value;
    const geocoder = new google.maps.Geocoder();
    geocoder.geocode({ address: city }, (results, status) => {
        if (status === 'OK') {
            map.setCenter(results[0].geometry.location);
            map.setZoom(13);

            const request = {
                location: results[0].geometry.location,
                radius: '5000',
                type: ['car_repair']
            };

            service = new google.maps.places.PlacesService(map);
            service.nearbySearch(request, (results, status) => {
                if (status === google.maps.places.PlacesServiceStatus.OK) {
                    for (let i = 0; i < results.length; i++) {
                        createMarker(results[i]);
                    }
                }
            });
        } else {
            Swal.fire({
                title: 'לא הצלחנו למצוא את העיר',
                text: 'נסו להזין שם עיר אחר',
                icon: 'info'
            });
        }
    });
}

function createMarker(place) {
    const marker = new google.maps.Marker({
        map: map,
        position: place.geometry.location
    });

    google.maps.event.addListener(marker, 'click', () => {
        infowindow.setContent(`
            <div><strong>${place.name}</strong><br>
            ${place.vicinity}</div>
        `);
        infowindow.open(map, marker);
    });
}


function showApprovedTag() {
    if (localStorage.getItem('UserCarlist') != null) {
        Cars = JSON.parse(localStorage.getItem('UserCarlist'))
        for (var i = 0; i < Cars.length; i++) {
            if (Cars[i].licensePlate == $("#carSelect").val()) {
                localStorage.setItem('Verifiedvehicle', Cars[i].isVerified);
            }
        }
    }
    if (localStorage.getItem('Verifiedvehicle') == 'true') {
        $("#approvedIMG").removeClass('hiddenApproved');
        $("#approvedIMG2").removeClass('hiddenApproved');//מסיר את הקלאס של ההסתרה

    }
    else {
        $("#approvedIMG").addClass('hiddenApproved');
        $("#approvedIMG2").addClass('hiddenApproved');//מסיר את הקלאס של ההסתרה

    }

}

function GetSelectedCarDetails(LPlate) {
    var misparRechev = LPlate;
    var resourceId = "053cea08-09bc-40ec-8f7a-156f0677aff3";
    var url = `https://data.gov.il/api/3/action/datastore_search?resource_id=${resourceId}&q=${misparRechev}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const records = data.result.records;
                localStorage.setItem('vehicleDetails', JSON.stringify(records));
            } else {
                console.error("Failed to fetch data:", data);

            }
        })
        .catch(error => {
            console.error("Error fetching data:", error);
            Swal.fire({ // במידה והרכב לא מופיע במערכת
                title: "מספר רישוי זה אינו מופיע במערכת",
                icon: "error"
            }).then(function () {
                $("#IDlicenseNum").val("");
            });
        });


}
