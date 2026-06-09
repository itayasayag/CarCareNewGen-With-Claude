function AddCarToListFromStorage() {
    var temp = localStorage.getItem('LogInUser');
    var tempUser = JSON.parse(temp);
    if (localStorage.getItem('UserCarlist') == null) {
        ReadUserCarByEmail(tempUser.email)
    }
    var StorageCarlist = JSON.parse(localStorage.getItem('UserCarlist'));
/*    userEmail = tempUser.email;*/
    var valueCounter = 1;/* משתנה רץ לאופשנס*/
    var str = "<select onchange='changeCarSelect()' class='select-car' id='carSelect' name='carSelect'> <option value='' disabled selected>הרכבים שלי</option>";
    if (StorageCarlist != null) {
        for (var i = 0; i < StorageCarlist.length; i++) {
            str += `<option value='${StorageCarlist[i].licensePlate}'>` + StorageCarlist[i].nickName + '</option>';
        }
    }
    str += "<option style='background-color: #99BFD2' value='addCar'>הוסף רכב+ </option></select>";
    document.getElementById("SelectDiv").innerHTML = str;


}


function AddCarToAlertsListFromStorage() { //טיפה שונה מהוספה רגילה בגלל העיצוב הוספה בעמוד התראות
    var temp = localStorage.getItem('LogInUser');
    var StorageCarlist = JSON.parse(localStorage.getItem('UserCarlist'));
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
    console.log(str);
    document.getElementById("SelectDiv").innerHTML = str;
}


function ReadUserCarByEmail(email)
{
    api = server + "/api/UserCar/"+email;
    ajaxCall("GET", api, "", getSCB, getECB);
}

function getSCB(suc) {
    localStorage.setItem('UserCarlist', JSON.stringify(suc))
}

function getECB(err) {
    console.log(err);
}

function SaveCarsToLocalStorage(CarList) {
    localStorage.setItem('UserCarlist', JSON.stringify(CarList))
}

//קריאת רשימת טיפולים של משתמש ללוקאל
function ReadUserCarCareByEmail(email) {
    api = server + "/api/Log_Record/" + email;
    ajaxCall("GET", api, "", getCCSCB, getCCECB);
}

function getCCSCB(suc) {
    localStorage.setItem('UserCarCarelist', JSON.stringify(suc))
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
            alert('Geocode was not successful for the following reason: ' + status);
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

