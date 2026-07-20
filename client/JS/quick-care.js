(function () {
    "use strict";

    var GARAGE_RESOURCE = "bb68386a-a331-4bbc-b668-bba2766d517d";
    var GARAGE_URL = "https://data.gov.il/api/3/action/datastore_search";
    var cityList = [];
    var garageList = [];
    var citiesLoading = false;
    var dataLoaded = false;
    var invoiceFileName = "ללא תמונה";

    $(function () {
        var $plus = $("#plussign");
        if (!$plus.length) return;

        $plus.closest(".footer").addClass("quick-care-launcher");

        // HomePage already contains the form and its existing, tested flow.
        // Enhance it with the shared card motion and drag gesture only.
        if ($("#quickCareSheet").length) {
            bindDragToClose(function () {
                if (typeof window.closeQuickCareSheet === "function") {
                    window.closeQuickCareSheet();
                }
            });
            return;
        }

        injectSheet();
        bindEvents();
        bindDragToClose(closeSheet);
        window.openQuickCareSheet = openSheet;
        window.closeQuickCareSheet = closeSheet;
    });

    function injectSheet() {
        $("body").append(`
            <div id="quickCareBackdrop" class="quick-care-backdrop"></div>
            <section id="quickCareSheet" class="quick-care-sheet" aria-hidden="true" aria-label="תיעוד טיפול">
                <div class="quick-care-handle" role="button" aria-label="משוך למטה לסגירה"></div>
                <div class="quick-care-header">
                    <button type="button" id="quickCareClose" class="quick-care-close" aria-label="סגור">×</button>
                    <h2 id="h2carcare">תיעוד טיפול</h2>
                </div>
                <div class="quick-care-body">
                    <input type="file" id="quickImgupload" class="is-hidden" />
                    <div id="AddCareDiv">
                        <div class="row">
                            <div class="col-3"><img class="img-care" src="images/car (3).png" alt="רכב" /></div>
                            <div class="col-9 rounded-input" id="QuickSelectDiv"></div>
                        </div>
                        <div class="row">
                            <div class="col-3"><img class="img-care" src="images/calendar.png" alt="תאריך" /></div>
                            <div class="col-9 rounded-input"><input id="quick-date-input" type="date" name="date" /></div>
                        </div>
                        <div class="row">
                            <div class="col-3"><img class="img-care" src="images/odometer-for-kilometers-and-speed-control (2).png" alt="קילומטראז'" /></div>
                            <div class="col-9 rounded-input"><input id="quick-kilometers" type="number" inputmode="numeric" pattern="[0-9]*" name="kilometrs" placeholder="קילומטראז'" /></div>
                        </div>
                        <div class="row" id="quickCareTypeRow">
                            <div class="col-3"><img class="img-care" src="images/wrench (2).png" alt="סוג טיפול" /></div>
                            <div class="col-9 rounded-input" id="QuickType">
                                <select class="carcare-select" disabled><option>טוען סוגי טיפול...</option></select>
                            </div>
                        </div>
                        <div class="row" id="quickLocationRow">
                            <div class="col-3"><img class="img-care" src="images/location.png" alt="מיקום" /></div>
                            <div class="col-9 rounded-input" id="QuickCityList">
                                <input type="text" class="carcare-select" id="quickCitySearchInput" placeholder="עיר" autocomplete="off" />
                                <div id="quickCityOptionsList" class="garage-options-list"></div>
                            </div>
                            <div class="col-6 rounded-input is-hidden" id="QuickGarageList">
                                <input type="text" class="carcare-select" id="quickGarageSearchInput" placeholder="תחילה בחר עיר" disabled autocomplete="off" />
                                <input type="hidden" id="quickSelectedgarage" value="" />
                                <div id="quickGarageOptionsList" class="garage-options-list"></div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-3"><img class="img-care" src="images/coins.png" alt="עלות" /></div>
                            <div class="col-9 rounded-input"><input id="quick-cost" type="number" inputmode="decimal" name="cost" placeholder="עלות" /></div>
                        </div>
                        <div class="row">
                            <div class="col-3"><img class="img-care" src="images/notes (1).png" alt="הערה" /></div>
                            <div class="col-9 rounded-input"><textarea id="quick-notes" name="addnotes" placeholder="הוסף הערה" rows="4" cols="30"></textarea></div>
                        </div>
                        <div id="quickUplodePicture" class="receipt-upload">
                            <img id="quickUploadrecipt" src="images/uploadrecipt.png" alt="חשבונית" />
                            <span>הוסף חשבונית</span>
                        </div>
                    </div>
                </div>
                <div class="quick-care-actions">
                    <button type="button" class="btn btn-primary rounded-pill" id="quickCareAddButton">הוסף תיעוד</button>
                </div>
            </section>`);
    }

    function bindEvents() {
        $("#plussign").on("click.quickCare", function (event) {
            event.preventDefault();
            event.stopImmediatePropagation();
            openSheet();
        });
        $("#quickCareBackdrop, #quickCareClose").on("click", closeSheet);
        $("#quickCareAddButton").on("click", addCarCare);
        $("#quickUplodePicture").on("click", function () { $("#quickImgupload").trigger("click"); });
        $(document).on("change", "#quickSelectedtype", changeCareType);

        $(document).on("input", "#quickCitySearchInput", function () {
            var query = $(this).val().trim();
            renderCityOptions((query ? cityList.filter(function (city) {
                return city.indexOf(query) !== -1;
            }) : cityList).slice(0, 40));
        });
        $(document).on("focus", "#quickCitySearchInput", function () {
            if (!cityList.length) {
                $("#quickCityOptionsList").html("<div class='garage-option-empty'>" +
                    (citiesLoading ? "טוען ערים..." : "לא נטענו ערים") + "</div>").show();
                return;
            }
            renderCityOptions(cityList.slice(0, 40));
        });
        $(document).on("click", "#quickCityOptionsList .garage-option", function () {
            var city = $(this).data("city") || $(this).text().trim();
            $("#quickCitySearchInput").val(city);
            $("#quickCityOptionsList").hide();
            $("#QuickCityList").removeClass("col-9").addClass("col-3");
            $("#QuickGarageList").removeClass("is-hidden");
            $("#quickGarageSearchInput").val("").attr("placeholder", "טוען מוסכים...").prop("disabled", true);
            $("#quickSelectedgarage").val("");
            readGarages(city);
        });
        $(document).on("focus", "#quickGarageSearchInput", function () {
            if (!$(this).prop("disabled")) renderGarageOptions(garageList);
        });
        $(document).on("input", "#quickGarageSearchInput", function () {
            var query = $(this).val().trim();
            $("#quickSelectedgarage").val("");
            renderGarageOptions(query ? garageList.filter(function (garage) {
                return (garage.shem_mosah || "").indexOf(query) !== -1;
            }) : garageList);
        });
        $(document).on("click", "#quickGarageOptionsList .garage-option", function () {
            $("#quickSelectedgarage").val($(this).data("id"));
            $("#quickGarageSearchInput").val($(this).data("name"));
            $("#quickGarageOptionsList").hide();
        });
        $(document).on("click", function (event) {
            if (!$(event.target).closest("#QuickCityList").length) $("#quickCityOptionsList").hide();
            if (!$(event.target).closest("#QuickGarageList").length) $("#quickGarageOptionsList").hide();
        });
    }

    function openSheet() {
        renderCarSelect();
        if (!dataLoaded) {
            dataLoaded = true;
            readCities();
            readCareTypes();
        }
        $("#quickCareSheet, #quickCareBackdrop").addClass("is-open");
        $("#quickCareSheet").attr("aria-hidden", "false");
        $("body").addClass("quick-care-open");
    }

    function closeSheet() {
        $("#quickCareSheet, #quickCareBackdrop").removeClass("is-open");
        $("#quickCareSheet").attr("aria-hidden", "true").css("transform", "");
        $("body").removeClass("quick-care-open");
        $("#quickCityOptionsList, #quickGarageOptionsList").hide();
    }

    function bindDragToClose(closeHandler) {
        var sheet = document.getElementById("quickCareSheet");
        var handle = sheet && sheet.querySelector(".quick-care-handle");
        if (!sheet || !handle || handle.dataset.dragReady === "true") return;

        handle.dataset.dragReady = "true";
        var dragging = false;
        var startY = 0;
        var lastY = 0;
        var startTime = 0;

        handle.addEventListener("pointerdown", function (event) {
            if (!sheet.classList.contains("is-open")) return;
            dragging = true;
            startY = event.clientY;
            lastY = startY;
            startTime = Date.now();
            sheet.classList.add("is-dragging");
            handle.setPointerCapture(event.pointerId);
        });
        handle.addEventListener("pointermove", function (event) {
            if (!dragging) return;
            lastY = event.clientY;
            var distance = Math.max(0, lastY - startY);
            sheet.style.transform = "translate(-50%, " + distance + "px)";
        });
        function finishDrag(event) {
            if (!dragging) return;
            dragging = false;
            var distance = Math.max(0, lastY - startY);
            var velocity = distance / Math.max(1, Date.now() - startTime);
            sheet.classList.remove("is-dragging");
            sheet.style.transform = "";
            if (distance > 72 || velocity > 0.55) closeHandler();
            if (event.pointerId !== undefined && handle.hasPointerCapture(event.pointerId)) {
                handle.releasePointerCapture(event.pointerId);
            }
        }
        handle.addEventListener("pointerup", finishDrag);
        handle.addEventListener("pointercancel", finishDrag);
    }

    function getCars() {
        if (typeof window.GetCachedCarList === "function") return window.GetCachedCarList() || [];
        try { return JSON.parse(localStorage.getItem("UserCarlist")) || []; }
        catch (error) { return []; }
    }

    function renderCarSelect() {
        var cars = getCars();
        var html = "<select class='carcare-select quick-carcare-select' id='quickCarSelect' name='quickCarSelect'>" +
            "<option value='' disabled selected>בחר רכב</option>";
        cars.forEach(function (car) {
            html += "<option value='" + car.licensePlate + "'>" + car.nickName + "</option>";
        });
        html += "<option style='background-color:#99BFD2' value='addCar'>הוסף רכב+ </option></select>";
        $("#QuickSelectDiv").html(html);

        var selected = selectedCarFromStorage();
        if (selected && cars.some(function (car) { return String(car.licensePlate) === String(selected); })) {
            $("#quickCarSelect").val(selected);
        }
        $("#quickCarSelect").on("change", changeCar);
    }

    function selectedCarFromStorage() {
        try { return JSON.parse(localStorage.getItem("SelectedCar")); }
        catch (error) { return null; }
    }

    function changeCar() {
        var selected = $("#quickCarSelect").val();
        if (selected === "addCar") {
            Swal.fire({
                title: "האם ברצונך להוסיף רכב חדש?",
                icon: "question",
                showCancelButton: true,
                confirmButtonColor: "#235978",
                cancelButtonColor: "#d33",
                confirmButtonText: "הוסף רכב חדש",
                cancelButtonText: "בטל"
            }).then(function (result) {
                if (result.isConfirmed) window.location.href = "addCarPage.html";
            });
            $("#quickCarSelect").prop("selectedIndex", 0);
            return;
        }

        localStorage.setItem("SelectedCar", JSON.stringify(selected));
        $("#carSelect").val(selected);
        if (typeof window.GetSelectedCarDetails === "function") window.GetSelectedCarDetails(selected);
        if (typeof window.showApprovedTag === "function") window.showApprovedTag();
    }

    function readCities() {
        citiesLoading = true;
        fetch(GARAGE_URL + "?resource_id=" + GARAGE_RESOURCE + "&fields=yishuv&limit=32000")
            .then(function (response) { return response.json(); })
            .then(function (data) {
                citiesLoading = false;
                if (!data.success) return;
                var seen = {};
                cityList = [];
                data.result.records.forEach(function (record) {
                    var city = (record.yishuv || "").trim();
                    if (city && !seen[city]) {
                        seen[city] = true;
                        cityList.push(city);
                    }
                });
                cityList.sort(function (a, b) { return a.localeCompare(b, "he"); });
            })
            .catch(function (error) {
                citiesLoading = false;
                console.error("gov city error", error);
            });
    }

    function renderCityOptions(list) {
        if (!list.length) {
            $("#quickCityOptionsList").hide();
            return;
        }
        $("#quickCityOptionsList").html(list.map(function (city) {
            return "<div class='garage-option' data-city='" + city.replace(/'/g, "&#39;") + "'>" + city + "</div>";
        }).join("")).show();
    }

    function readGarages(city) {
        fetch(GARAGE_URL + "?resource_id=" + GARAGE_RESOURCE + "&q=" + encodeURIComponent(city) + "&limit=32000")
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (!data.success) return;
                var seen = {};
                garageList = [];
                data.result.records.forEach(function (record) {
                    var id = record.mispar_mosah;
                    if ((record.yishuv || "").trim() === city && id && !seen[id]) {
                        seen[id] = true;
                        garageList.push({ id: id, shem_mosah: record.shem_mosah });
                    }
                });
                garageList.sort(function (a, b) {
                    return (a.shem_mosah || "").localeCompare(b.shem_mosah || "", "he");
                });
                $("#quickGarageSearchInput").prop("disabled", false).attr("placeholder", "חפש מוסך...");
                renderGarageOptions(garageList);
            })
            .catch(function (error) { console.error("gov garage error", error); });
    }

    function renderGarageOptions(list) {
        if (!list.length) {
            $("#quickGarageOptionsList").html("<div class='garage-option-empty'>לא נמצאו מוסכים</div>").show();
            return;
        }
        $("#quickGarageOptionsList").html(list.map(function (garage) {
            return "<div class='garage-option' data-id='" + garage.id + "' data-name='" +
                String(garage.shem_mosah || "").replace(/'/g, "&#39;") + "'>" + garage.shem_mosah + "</div>";
        }).join("")).show();
    }

    function readCareTypes() {
        ajaxCall("GET", server + "/api/caretypes/", "", function (list) {
            var html = "<select class='carcare-select' id='quickSelectedtype' name='Type'>" +
                "<option value='' disabled selected>סוג טיפול</option>";
            list.forEach(function (care) {
                html += "<option value='" + care.careID + "'>" + care.careName + "</option>";
            });
            $("#QuickType").html(html + "</select>");
        }, function (error) {
            console.error("care type error", error);
        });
    }

    function changeCareType() {
        var careId = $("#quickSelectedtype").val();
        var careName = $("#quickSelectedtype option:selected").text().trim();
        var isInsurance = String(careId) === "10" || careName.indexOf("ביטוח") !== -1;
        $("#quickLocationRow").toggleClass("row-hidden", isInsurance);
        if (isInsurance) {
            $("#quickCityOptionsList, #quickGarageOptionsList").hide();
            $("#quickSelectedgarage, #quickGarageSearchInput, #quickCitySearchInput").val("");
        }
    }

    function addCarCare() {
        var user;
        try { user = JSON.parse(localStorage.getItem("LogInUser")); }
        catch (error) { user = null; }
        var selectedCar = $("#quickCarSelect").val() || selectedCarFromStorage();

        if (!selectedCar) {
            Swal.fire({ title: "לא נבחר רכב", text: "לא נבחר רכב עבורו נדרש להוסיף תיעוד", icon: "question" });
            return;
        }
        if (!user || !user.email) {
            Swal.fire({ title: "אינך מחובר למערכת", icon: "info" });
            return;
        }
        if (!$("#quick-date-input").val() || !$("#quick-kilometers").val() ||
            !$("#quickSelectedtype").val() || !$("#quick-cost").val()) {
            Swal.fire({
                title: "מידע חסר",
                text: "ודא/י שמילאת תאריך, קילומטראז', סוג טיפול ועלות",
                icon: "question"
            });
            return;
        }

        uploadInvoice(selectedCar, user);
        var garageId = $("#quickSelectedgarage").val();
        var garageName = "";
        var garage = garageList.find(function (item) { return String(item.id) === String(garageId); });
        if (garage) garageName = garage.shem_mosah || "";

        var logRecord = {
            userEmail: user.email,
            licensePlate: parseInt(selectedCar, 10),
            recordDate: $("#quick-date-input").val(),
            currentKM: $("#quick-kilometers").val(),
            garageID: garageId && garageId !== "0" ? garageId : null,
            careID: $("#quickSelectedtype").val(),
            cost: $("#quick-cost").val(),
            notes: $("#quick-notes").val(),
            invoiceFileName: invoiceFileName,
            careName: "",
            garageName: garageName
        };

        ajaxCall("POST", server + "/api/logs/", JSON.stringify(logRecord), postSuccess, postError);
    }

    function postSuccess() {
        var user = JSON.parse(localStorage.getItem("LogInUser"));
        if (typeof window.ReadUserCarCareByEmail === "function") window.ReadUserCarCareByEmail(user.email);
        Swal.fire({
            title: "נוסף תיעוד טיפול",
            icon: "success",
            showDenyButton: true,
            confirmButtonText: "חזרה לדף הבית",
            denyButtonText: "הוסף טיפול נוסף",
            confirmButtonColor: "#1E5A82",
            denyButtonColor: "#2C7DB3"
        }).then(function (result) {
            if (result.isDenied) {
                resetForm();
                return;
            }
            window.location.href = "HomePage.html";
        });
    }

    function postError(error) {
        console.error(error);
        Swal.fire({ title: "כשל בהוספת טיפול", text: "אנא בדוק את הפרטים שהוזנו במערכת", icon: "warning" });
    }

    function resetForm() {
        $("#quick-date-input, #quick-kilometers, #quick-cost, #quick-notes, #quickCitySearchInput, #quickSelectedgarage").val("");
        $("#quickSelectedtype").prop("selectedIndex", 0);
        $("#quickLocationRow").removeClass("row-hidden");
        $("#QuickCityList").removeClass("col-3").addClass("col-9");
        $("#QuickGarageList").addClass("is-hidden");
        $("#quickGarageSearchInput").val("").attr("placeholder", "תחילה בחר עיר").prop("disabled", true);
        $("#quickGarageOptionsList").hide();
        $("#quickUploadrecipt").attr("src", "images/uploadrecipt.png");
        $("#quickImgupload").val("");
        invoiceFileName = "ללא תמונה";
    }

    function uploadInvoice(selectedCar, user) {
        var input = $("#quickImgupload").get(0);
        if (!input || !input.files.length) return;

        var data = new FormData();
        Array.prototype.forEach.call(input.files, function (file) {
            var extension = file.name.split(".").pop();
            invoiceFileName = user.email + "_" + parseInt(selectedCar, 10) + "invoice_" + Date.now() + "." + extension;
            data.append("files", new File([file], invoiceFileName, { type: file.type }));
        });

        var uploadUrl = server + "/api/upload/";
        $.ajax({
            type: "POST",
            url: uploadUrl,
            contentType: false,
            processData: false,
            data: data,
            success: function (result) {
                var files = result && result.uploadedFiles ? result.uploadedFiles : result;
                var fileName = Array.isArray(files) ? files[files.length - 1] : files;
                if (fileName) $("#quickUploadrecipt").attr("src", uploadUrl + fileName);
                Swal.fire({ title: "העלאת קובץ בוצעה בהצלחה!", icon: "success" });
            },
            error: function (error) { console.error("invoice upload error", error); }
        });
    }
})();
