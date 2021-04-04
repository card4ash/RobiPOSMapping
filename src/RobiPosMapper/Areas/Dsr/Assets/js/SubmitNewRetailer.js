//New Retailer form submit

$(function () {
    $("#SubmitRetailer").click(function (e) {
        e.preventDefault();

        var fd = new FormData();
        var ErrorFound = false; var ErrorText = "";

        var RetailerName = $.trim($('#RetailerName').val());
        if (RetailerName.length == 0) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(0);">Please enter retailer name.</a>';
        }
        else {
            if (RetailerName.length > 100) {
                ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(0);">Retailer name is too long.</a>';
            }
            else {
                fd.append('RetailerName', RetailerName);
            }
        }

        var RetailerAddress = $.trim($('#RetailerAddress').val());
        if (RetailerAddress.length == 0) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(0);">Please enter retailer address.</a>';
        }
        else {
            if (RetailerAddress.length>250) {
                ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(0);">Retailer address is too long.</a>';
            }
            else {
                fd.append('RetailerAddress', RetailerAddress);
            }
        }

        //POS category check
        var PosCategoryId = $('#PosCategoryId').val();
        if (PosCategoryId == '' || PosCategoryId == '-1') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(0);">Please select POS category.</a>';
        }
        else {
            fd.append('PosCategoryId', PosCategoryId);
        }

        //Checking EL Pos checkbox and textboxes (if any)--------------------->
        if ($IsElPosCheckbox.is(':checked')) {

            var ElMsisdnInputQuantity = $('.ElMsisdnInput').length;
            if (ElMsisdnInputQuantity == 0) {
                ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">Please add EL MSISDN.</a>';
            }
            else {
                var ElMsisdnArray = [];
                var counter = 1;
                $('.ElMsisdnInput').each(function (i, item) {
                    var elMsisdn = $.trim($(item).val());
                    if (elMsisdn.length == 0) {
                        ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);"> Please enter EL MSISDN at position ' + counter + '.</a>';
                    }
                    else {
                        if (isNaN(elMsisdn)) {
                            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">EL MSISDN must be nemeric at position ' + counter + '.</a>';
                        }
                        else {

                            if (elMsisdn.length == 10) {
                                var firstTwoCharacters = elMsisdn.substring(0, 2);

                                if (firstTwoCharacters == '18') {
                                    ElMsisdnArray.push(elMsisdn);
                                }
                                else {
                                    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">You entered an invalid EL MSISDN. It must start with 18 at position ' + counter + '.</a>';
                                }
                            }
                            else {
                                if (elMsisdn.length == 11) {
                                    var firstThreeCharacters = elMsisdn.substring(0, 3);
                                    if (firstThreeCharacters == '018') {
                                        ElMsisdnArray.push(elMsisdn);
                                    }
                                    else {
                                        ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">You entered an invalid EL MSISDN. It must start with 018 at position ' + counter + '.</a>';
                                    }
                                }
                                else {
                                    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">You entered an invalid EL MSISDN. It contains invalid number of digits at position ' + counter + '.</a>';
                                }
                            }
                        }
                    }
                    counter++;
                });
                fd.append("IsElMsisdn", true);

                fd.append("ElMsisdn", ElMsisdnArray);
            }
        }
        else {
            fd.append("IsElMsisdn", false);
        }
        //<------------------------


        //Checking SIM Pos checkbox and textbox --------------------->
        if ($IsSimPosCheckBox.is(':checked')) {

            var SimPosCodeInputQuantity = $('.SimPosCodeInput').length;
            if (SimPosCodeInputQuantity == 0) {
                ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">Please add SIM POS code.</a>';
            }
            else {
                var SimPosCodeArray = [];
                var counter = 1;
                $('.SimPosCodeInput').each(function (i, item) {
                    var simPosCode = $.trim($(item).val());
                    if (simPosCode.length == 0) {
                        ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);"> Please enter SIM POS code at position ' + counter + '.</a>';
                    }
                    else {
                        if (simPosCode.indexOf(',') > -1) {
                            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(1);">Remove comma(,) from SIM POS code at position ' + counter + '.</a>';
                        }
                        else {
                            SimPosCodeArray.push(simPosCode);
                        }
                    }
                    counter++;
                });
                fd.append("IsSimPos", true);

                fd.append("SimPosCode", SimPosCodeArray);
            }
        }
        else {
            fd.append("IsSimPos", false);
        }//<--------------------

        fd.append("IsScPos", $IsScPosCheckBox.is(':checked'));

        fd.append("HasTradeLicense", $HasTradeLicense.is(':checked'));
       


        //if ($ThanaCombo.val() == '' || $ThanaCombo.val() == '-1') {
        //    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Please select thana.</a>';
        //}
        //else {
        //    fd.append('RegionId', $RegionCombo.val());
        //    fd.append('AreaId', $AreaCombo.val());
        //    fd.append('ThanaId', $ThanaCombo.val());
        //    fd.append('WardId', $WardCombo.val());
        //    fd.append('MauzaId', $MauzaCombo.val());
        //    fd.append('VillageId', $VillageCombo.val());
        //}

        //Region to Thana
        if ($ThanaCombo.val() == '' || $ThanaCombo.val() == '-1') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Please select thana.</a>';
        }
        else {
            fd.append('RegionId', $RegionCombo.val());
            fd.append('AreaId', $AreaCombo.val());
            fd.append('ThanaId', $ThanaCombo.val());
        }

        //Union/Ward
        if ($WardCombo.val() == '') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Union/Ward information can not be empty.</a>';
        }
        else {
            fd.append('WardId', $WardCombo.val());
        }

        //Mauza/Mohalla
        if ($MauzaCombo.val() == '') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Mauza/Mohalla information can not be empty.</a>';
        }
        else {
            //fd.append('MauzaId', $MauzaCombo.val());
            if (CheckAccuracy == false) {
                if ($MauzaCombo.val() == '0') {
                    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Please select mauza/mohalla.</a>';
                }
                else {
                    fd.append('MauzaId', $MauzaCombo.val());
                }
            }
            else {
                fd.append('MauzaId', $MauzaCombo.val());
            }
        }

        //Village
        if ($VillageCombo.val() == '') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(2);">Village information can not be empty.</a>';
        }
        else {
            fd.append('VillageId', $VillageCombo.val());
        }

        //checking PosStructureId
        var $PosStructureId = $('#PosStructureId').val();
        if ($PosStructureId == -1) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(3);">Please select POS structure type.</a>';
        }
        else {
            fd.append('PosStructureId', $PosStructureId);
        }

        //checking VisitDayId
        var $VisitDayId = $('#VisitDayId').val();
        if ($VisitDayId == -1) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(3);">Please select visit day.</a>';
        }
        else {
            fd.append('VisitDayId', $VisitDayId);
        }

        //checking ShopSignageId
        var $ShopSignageId = $('#ShopSignageId').val();
        if ($ShopSignageId == -1) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(3);">Please select shop signage.</a>';
        }
        else {
            fd.append('ShopSignageId', $ShopSignageId);
        }

        //checking ShopTypeId
        var $ShopTypeId = $('#ShopTypeId').val();
        if ($ShopTypeId == -1) {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(3);">Please select shop type.</a>';
        }
        else {
            fd.append('ShopTypeId', $ShopTypeId);
        }

        //checking Locality
        fd.append("IsApartments", $('#IsApartments').is(':checked'));
        fd.append("IsSlums", $('#IsSlums').is(':checked'));
        fd.append("IsSemiUrbunHousing", $('#IsSemiUrbunHousing').is(':checked'));
        fd.append("IsRuralHousing", $('#IsRuralHousing').is(':checked'));
        fd.append("IsShoppingMall", $('#IsShoppingMall').is(':checked'));
        fd.append("IsRetailHub", $('#IsRetailHub').is(':checked'));
        fd.append("IsMobileDeviceMarket", $('#IsMobileDeviceMarket').is(':checked'));
        fd.append("IsBazaar", $('#IsBazaar').is(':checked'));
        fd.append("IsOfficeArea", $('#IsOfficeArea').is(':checked'));
        fd.append("IsGarmentsMajorityArea", $('#IsGarmentsMajorityArea').is(':checked'));
        fd.append("IsGeneralIndustrialArea", $('#IsGeneralIndustrialArea').is(':checked'));
        fd.append("IsUrbanTransitPoints", $('#IsUrbanTransitPoints').is(':checked'));
        fd.append("IsRuralTransitPoints", $('#IsRuralTransitPoints').is(':checked'));
        fd.append("IsUrbanYouthHangouts", $('#IsUrbanYouthHangouts').is(':checked'));
        fd.append("IsSemiUrbanYouthHangouts", $('#IsSemiUrbanYouthHangouts').is(':checked'));
        fd.append("IsRuralYouthHangouts", $('#IsRuralYouthHangouts').is(':checked'));
        fd.append("IsTouristDestinations", $('#IsTouristDestinations').is(':checked'));

        //checking retailer photo
        var RetailerPhotoSrc = $('#RetailerPhotoPreview').attr('src');

        if (RetailerPhotoSrc === '#' || RetailerPhotoSrc === '/Photos/RetailerPhoto/default.png') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(5);">Please add retailer photo.</a>';
        }
        else {
            var RetailerPhoto = document.getElementById('RetailerPhotoInput').files[0];
            fd.append("RetailerPhoto", RetailerPhoto);
        }

        //checking QR code
        var QrValue = $('#QrId').val();
        if (QrValue.toString() == '') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(6);">Please tag/enter QR code.</a>';
        }
        else {
            if (isNaN(QrValue)) {
                ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(6);">Invalid QR code. It must be numeric.</a>';
            }
            else {
                if (QrValue.length!=6) {
                    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(6);">Invalid QR code. It must have 6 digits.</a>';
                }
                else {
                    fd.append('QrValue', QrValue);
                }
            }
        }

        var Latitude = $('#Latitude').val();
        var Longitude = $('#Longitude').val();
        var Accuracy = $('#Accuracy').val();

        if (Latitude == '' || Longitude == '') {
            ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(7);">Please locate your position.</a>';

        }
        else {

            if (Accuracy>20.0) {
                //ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(7);">Geolocation accuracy is out of limit.</a>';
                if (CheckAccuracy == true) {
                    ErrorFound = true; ErrorText += '<a href="#" onclick="swiperParent.swipeTo(7);">Geolocation accuracy is out of limit.</a>';
                }
                else {
                    fd.append('Latitude', Latitude);
                    fd.append('Longitude', Longitude);
                    fd.append('Accuracy', Accuracy);
                }
            }
            else {
                fd.append('Latitude', Latitude);
                fd.append('Longitude', Longitude);
                fd.append('Accuracy', Accuracy);
            }
           
        }

        fd.append("Remarks", $Remarks.val());

        fd.append("SessionId", $('#SessionId').val());

        //raise error(s), if any
        if (ErrorFound) {
            $('#ErrorDetails').html(ErrorText); return;
        }
        else {
            $('#ErrorDetails').html('');
        }

        var urlData = '../../../NewRetailer/CreateNewRetailer';
        $.ajax({
            url: urlData,
            type: "POST",
            data: fd,
            processData: false,  // tell jQuery not to process the data
            contentType: false,   // tell jQuery not to set contentType
            beforeSend: function () {
                $('#SubmitRetailer').attr('disabled', 'disabled');
                $('#SubmitRetailer').html('Working...');
            },
            success: function (response) {
                if (response.IsError == true) {
                    if (response.IsSessionError==true) {
                        window.location = response.url;
                    }
                    else {
                        $.modal('<h3 style="color:red;">' + response.ErrorDetails + '</h3>');
                    }
                }
                else {
                    //alert('New retailer data successfully saved.');
                    $.modal('<h3 style="color:green;">New retailer data successfully saved.</h3>');
                    ResetForm();
                }
            },
            error: function (request, status, error) {
                $.modal('<h3 style="color:red;">' + ajaxErrorMessage + '</h3>');
            },
            complete: function () {
                $('#SubmitRetailer').removeAttr("disabled");
                $('#SubmitRetailer').html('Submit');
            }
        });
    });
});