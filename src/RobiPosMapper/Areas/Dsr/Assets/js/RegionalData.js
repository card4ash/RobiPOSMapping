$(function () {

    // GetAllRegions();
    $RegionCombo.change(function () {
        var selectedRegionId = $RegionCombo.val(); //($RegionCombo + 'option:selected').attr('value');
        switch (selectedRegionId) {
            case "-1":
                $AreaCombo.empty().append('<option value="-1">n/a</option>');
                $ThanaCombo.empty().append('<option value="-1">n/a</option>');
                $WardCombo.empty().append('<option value="-1">n/a</option>');
                $MauzaCombo.empty().append('<option value="-1">n/a</option>');
                $VillageCombo.empty().append('<option value="-1">n/a</option>');
                break;
            default:
                GetRegionSpecificAreas(selectedRegionId);
                $ThanaCombo.empty().append('<option value="-1">n/a</option>');
                $WardCombo.empty().append('<option value="-1">n/a</option>');
                $MauzaCombo.empty().append('<option value="-1">n/a</option>');
                $VillageCombo.empty().append('<option value="-1">n/a</option>');
                break;
        }

        
    });

    //Area combo change handler
    $AreaCombo.change(function () {
        var selectedAreaId = $AreaCombo.val(); //($RegionCombo + 'option:selected').attr('value');
        switch (selectedAreaId) {
            case "-1":
                $ThanaCombo.empty().append('<option value="-1">n/a</option>');
                $WardCombo.empty().append('<option value="-1">n/a</option>');
                $MauzaCombo.empty().append('<option value="-1">n/a</option>');
                $VillageCombo.empty().append('<option value="-1">n/a</option>');
                break;
            default:
                GetAreaSpecificThanas(selectedAreaId);
                $WardCombo.empty().append('<option value="-1">n/a</option>');
                $MauzaCombo.empty().append('<option value="-1">n/a</option>');
                $VillageCombo.empty().append('<option value="-1">n/a</option>');
                break;
        }
    });

    //Thana combo change handler
    $ThanaCombo.change(function () {
        var selectedThanaId = $ThanaCombo.val(); //($RegionCombo + 'option:selected').attr('value');
        switch (selectedThanaId) {
            case "-1":
                $WardCombo.empty().append('<option value="0">n/a</option>');
                $MauzaCombo.empty().append('<option value="0">n/a</option>');
                $VillageCombo.empty().append('<option value="0">n/a</option>');
                break;
            default:
                GetThanaSpecificWards(selectedThanaId);
                $MauzaCombo.empty().append('<option value="0">n/a</option>');
                $VillageCombo.empty().append('<option value="0">n/a</option>');
                break;
        }
    });

    //Ward,Union change handler
    $WardCombo.change(function () {
        var selectedWardId = $WardCombo.val();
        switch (selectedWardId) {
            case "0":
                $MauzaCombo.empty().append('<option value="0">n/a</option>'); $VillageCombo.empty().append('<option value="0">n/a</option>');
                break;
            default:
                GetWardSpecificMauzas(selectedWardId);
                $VillageCombo.empty().append('<option value="0">n/a</option>');
                break;
        }
    });

    //Mauza/Moholla change handler
    $MauzaCombo.change(function () {
        var selectedMauzaId = $MauzaCombo.val();
        switch (selectedMauzaId) {
            case "0":
                $VillageCombo.empty().append('<option value="0">n/a</option>');
                break;
            default:
                GetMauzaSpecificVillages(selectedMauzaId);
                break;
        }
    });

});



//Region to village data loader 

    function GetAllRegions() {
        var urlData = '../../../Common/GetAllRegions';
        $.ajax({
            type: "POST",
            url: urlData,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                $RegionCombo.empty();
                if (result.status === 'success') {
                    var regionCount = result.data.length;
                    if (regionCount > 0) {
                        $RegionCombo.append('<option value="-1">select a region</option>');
                        $.each(result.data, function () {
                            $RegionCombo.append('<option value="' + this.RegionId + '">' + this.RegionName + '</option>');
                        });

                    } else { $RegionCombo.append('<option value="-1">Regions not found</option>'); }
                }
                else {
                    $RegionCombo.append('<option value="-1">Errors in loading data</option>');
                }
            },
            error: function (request, status, error) {
                $RegionCombo.empty();
                $RegionCombo.append('<option value="-1">Errors in loading data</option>');
            }
        });
    }

function GetRegionSpecificAreas(regionId) {
    $.ajax({
        type: "POST",
        url: '../../../Common/GetRegionSpecificAreas',
        contentType: "application/json; charset=utf-8",
        data: "{'regionId':'" + regionId + "'}",
        dataType: "json",
        beforeSend: function () {
            $AreaCombo.empty().append('<option value="-1">loading areas..</option>');
        },
        success: function (result) {
            $AreaCombo.empty();
            if (result.status === 'success') {
                var areaCount = result.data.length;
                if (areaCount > 0) {
                    $AreaCombo.empty().append('<option value="-1">select an area</option>');
                    $.each(result.data, function () {
                        $AreaCombo.append('<option value="' + this.AreaId + '">' + this.AreaName + '</option>');
                    });

                } else { $AreaCombo.empty().append('<option value="-1">areas not found</option>'); }
            }
            else {
                $AreaCombo.empty().append('<option value="-1">error in loading data</option>');
            }
        },
        error: function (request, status, error) {
            $AreaCombo.empty().append('<option value="-1">error in loading data</option>');
        }
    });



}

function GetAreaSpecificThanas(areaId) {
    $.ajax({
        type: "POST",
        url: '../../../Common/GetAreaSpecificThanas',
        contentType: "application/json; charset=utf-8",
        data: "{'areaId':'" + areaId + "'}",
        dataType: "json",
        beforeSend: function () {
            $ThanaCombo.empty().append('<option value="-1">loading thanas..</option>');
        },
        success: function (result) {
            $ThanaCombo.empty();
            if (result.status === 'success') {
                var dataCount = result.data.length;
                if (dataCount > 0) {
                    $ThanaCombo.append('<option value="-1">select a thana</option>');
                    $.each(result.data, function () {
                        $ThanaCombo.append('<option value="' + this.ThanaId + '">' + this.ThanaName + '</option>');
                    });

                } else { $ThanaCombo.append('<option value="-1">thanas not found</option>'); }
            }
            else {
                $ThanaCombo.append('<option value="-1">error in loading data</option>');
            }
        },
        error: function (request, status, error) {
            $ThanaCombo.empty();
            $ThanaCombo.append('<option value="-1">error in loading data</option>');
        }
    });



}

//Thana to Ward
function GetThanaSpecificWards(thanaId) {
    $.ajax({
        type: "POST",
        url: '../../../Common/GetThanaSpecificWards',
        contentType: "application/json; charset=utf-8",
        data: "{'thanaId':'" + thanaId + "'}",
        dataType: "json",
        beforeSend: function () {
            $WardCombo.empty().append('<option value="0">loading data..</option>');
        },
        success: function (result) {
            $WardCombo.empty();
            if (result.status === 'success') {
                var dataCount = result.data.length;
                if (dataCount > 0) {
                    $WardCombo.append('<option value="0">select an ward</option>');
                    $.each(result.data, function () {
                        $WardCombo.append('<option value="' + this.WardId + '">' + this.WardName + '</option>');
                    });

                } else { $WardCombo.append('<option value="0">wards not found</option>'); }
            }
            else {
                $WardCombo.append('<option value="0">error in loading data</option>');
            }
        },
        error: function (request, status, error) {
            $WardCombo.empty();
            $WardCombo.append('<option value="0">error in loading data</option>');
        }
    });
}

//Ward to Mauzas
function GetWardSpecificMauzas(wardId) {
    $.ajax({
        type: "POST",
        url: '../../../Common/GetWardSpecificMauzas',
        contentType: "application/json; charset=utf-8",
        data: "{'wardId':'" + wardId + "'}",
        dataType: "json",
        beforeSend: function () {
            $MauzaCombo.empty().append('<option value="0">loading data..</option>');
        },
        success: function (result) {
            $MauzaCombo.empty();
            if (result.status === 'success') {
                var dataCount = result.data.length;
                if (dataCount > 0) {
                    $MauzaCombo.append('<option value="0">select a mauza</option>');
                    $.each(result.data, function () {
                        $MauzaCombo.append('<option value="' + this.MauzaId + '">' + this.MauzaName + '</option>');
                    });
                } else { $MauzaCombo.append('<option value="0">mauzas not found</option>'); }
            }
            else {
                $MauzaCombo.append('<option value="0">error in loading data</option>');
            }
        },
        error: function (request, status, error) {
            $MauzaCombo.empty();
            $MauzaCombo.append('<option value="0">error in loading data</option>');
        }
    });
}

//Mauza to Villages
function GetMauzaSpecificVillages(mauzaId) {
    $.ajax({
        type: "POST",
        url: '../../../Common/GetMauzaSpecificVillages',
        contentType: "application/json; charset=utf-8",
        data: "{'mauzaId':'" + mauzaId + "'}",
        dataType: "json",
        beforeSend: function () {
            $VillageCombo.empty().append('<option value="0">loading villages..</option>');
        },
        success: function (result) {
            $VillageCombo.empty();
            if (result.status === 'success') {
                var dataCount = result.data.length;
                if (dataCount > 0) {
                    $VillageCombo.append('<option value="0">select a village</option>');
                    $.each(result.data, function () {
                        $VillageCombo.append('<option value="' + this.VillageId + '">' + this.VillageName + '</option>');
                    });
                } else { $VillageCombo.append('<option value="0">villages not found</option>'); }
            }
            else {
                $VillageCombo.append('<option value="0">error in loading data</option>');
            }
        },
        error: function (request, status, error) {
            $VillageCombo.empty();
            $VillageCombo.append('<option value="0">error in loading data</option>');
        }
    });
}

