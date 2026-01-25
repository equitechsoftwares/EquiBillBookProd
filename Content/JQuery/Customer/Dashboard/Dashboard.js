$(function () {

    $('.tblData').DataTable({
        lengthChange: false,
        searching: false,
        autoWidth: false,
        responsive: true,
        paging: false,
        bInfo: false
    });
});

var EnableSound = Cookies.get('SystemSetting').split('&')[4].split('=')[1];

var interval = null;

var month = new Date().getMonth()+1;
var year = new Date().getFullYear();
$('#ddlPurchaseMonth').val(month);
$('#ddlPurchaseYear').val(year);
$('#ddlSalesMonth').val(month);
$('#ddlSalesYear').val(year);

if (window.location.href.indexOf('login') != -1) {
    interval = setInterval(playSound, 500);
}

function playSound() {
    if (EnableSound == 'True') { document.getElementById('login').play(); }

    clearInterval(interval);
}

function fetchList() {
    var det = {
        BranchId: $('#ddlBranch').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Dashboard/DashboardFetch',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#p_div").html(data);
            //$('.chkIsActive').bootstrapToggle();
            $("#divLoading").hide();

            $('.tblData').DataTable({
                lengthChange: false,
                searching: false,
                autoWidth: false,
                responsive: false,
                paging: false,
                bInfo: false,
                "bDestroy": true
            });
            $("#thead").insertBefore(".table-body");
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchTopCustomersGraph() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        Type: $('#ddlCustomerGraph').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Dashboard/fetchTopCustomersGraph',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var model = data.Data.Dashboard.TopCustomers;
            var label = [];
            var data = [];
            $.each(model, function (key, value) {
                label.push(value.Name + '-' + value.MobileNo);
                data.push(value.TotalSales);
            });
            const ctx = document.getElementById('myChartCustomers');
            const myChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: label,
                    datasets: [{
                        label: 'Top 10 Customers',
                        data: data,
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.2)',
                            'rgba(54, 162, 235, 0.2)',
                            'rgba(255, 206, 86, 0.2)',
                            'rgba(75, 192, 192, 0.2)',
                            'rgba(153, 102, 255, 0.2)',
                            'rgba(255, 159, 64, 0.2)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)',
                            'rgba(54, 162, 235, 1)',
                            'rgba(255, 206, 86, 1)',
                            'rgba(75, 192, 192, 1)',
                            'rgba(153, 102, 255, 1)',
                            'rgba(255, 159, 64, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            grid: {
                                display: false,
                            },
                            beginAtZero: true
                        },
                        y: {
                            //grid: {
                            //    display: false,
                            //},
                            beginAtZero: true
                        }
                    }
                }
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function fetchTopItemsGraph() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        Type: $('#ddlItemGraph').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Dashboard/fetchTopItemsGraph',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var model1 = data.Data.Dashboard.TopItems;
            var label1 = [];
            var data1 = [];
            $.each(model1, function (key, value) {
                if (value.VariationName) {
                    label1.push(value.ItemName + '-' + value.VariationName + '-' + value.SKU);
                }
                else {
                    label1.push(value.ItemName + '-' + value.SKU);
                }
                data1.push(value.TotalSales);
            });
            const ctx1 = document.getElementById('myChartItems');
            const myChart1 = new Chart(ctx1, {
                type: 'pie',
                data: {
                    labels: label1,
                    datasets: [{
                        label: 'Top 10 Customers',
                        data: data1,
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.2)',
                            'rgba(54, 162, 235, 0.2)',
                            'rgba(255, 206, 86, 0.2)',
                            'rgba(75, 192, 192, 0.2)',
                            'rgba(153, 102, 255, 0.2)',
                            'rgba(255, 159, 64, 0.2)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)',
                            'rgba(54, 162, 235, 1)',
                            'rgba(255, 206, 86, 1)',
                            'rgba(75, 192, 192, 1)',
                            'rgba(153, 102, 255, 1)',
                            'rgba(255, 159, 64, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            grid: {
                                display: false,
                            },
                            beginAtZero: true
                        },
                        y: {
                            grid: {
                                display: false,
                            },
                            beginAtZero: true
                        }
                    }
                }
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function SalesMonthWiseGraph() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        Month: $('#ddlSalesMonth').val(),
        Year: $('#ddlSalesYear').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Dashboard/SalesMonthWiseGraph',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var model2 = data.Data.Dashboard.SalesMonthWise;
            var label2 = [];
            var data2 = [];
            $.each(model2, function (key, value) {
                label2.push(value.DayNo);
                data2.push(value.TotalSales);
            });
            const ctx2 = document.getElementById('myChartSales');
            const myChart2 = new Chart(ctx2, {
                type: 'line',
                data: {
                    labels: label2,
                    datasets: [{
                        label: 'Sales Month Wise',
                        data: data2,
                        fill: false,
                        borderColor: 'rgba(255, 99, 132, 1)',
                        tension: 0.1,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            //grid: {
                            //    display: false,
                            //},
                            beginAtZero: true
                        },
                        y: {
                            //grid: {
                            //    display: false,
                            //},
                            beginAtZero: true
                        }
                    }
                }
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function PurchaseMonthWiseGraph() {
    var det = {
        BranchId: $('#ddlBranch').val(),
        Month: $('#ddlPurchaseMonth').val(),
        Year: $('#ddlPurchaseYear').val(),
    };
    $("#divLoading").show();
    $.ajax({
        url: '/Dashboard/PurchaseMonthWiseGraph',
        datatype: "json",
        data: det,
        type: "post",
        success: function (data) {
            $("#divLoading").hide();
            var model3 = data.Data.Dashboard.PurchaseMonthWise;
            var label3 = [];
            var data3 = [];
            $.each(model3, function (key, value) {
                label3.push(value.DayNo);
                data3.push(value.TotalPurchase);
            });
            const ctx3 = document.getElementById('myChartPurchase');
            const myChart3 = new Chart(ctx3, {
                type: 'line',
                data: {
                    labels: label3,
                    datasets: [{
                        label: 'Purchase Month Wise',
                        data: data3,
                        fill: false,
                        borderColor: 'rgb(75, 192, 192)',
                        tension: 0.1,
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            //grid: {
                            //    display: false,
                            //},
                            beginAtZero: true
                        },
                        y: {
                            //grid: {
                            //    display: false,
                            //},
                            beginAtZero: true
                        }
                    }
                }
            });
        },
        error: function (xhr) {
            $("#divLoading").hide();
        }
    });
};

function OpenUrl(url) {
    //window.open(url + "?BranchId=" + $('#ddlBranch').val(),'_blank');
    location.href =url + "?BranchId=" + $('#ddlBranch').val();
}

function openDemoAccount() {
    if (confirm('You will be logged out and login to our demo account. You can always login back to you account using your existing email id and password. Do you want to continue?')) {
        location.href = "/home/demo";
    }
}