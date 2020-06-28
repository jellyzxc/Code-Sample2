
var statusList = ['已作废', '待签批', '已签批', '已分发', '已分派', '已整理提交', '已初审', '已复审', '已终审交付'];

var TableInit = function () {
    queryParams = function (params) {
        var temp = {
            limit: params.limit,
            offset: params.offset / params.limit,
            sort: params.sort,
            order: params.order,
            Title: queryForm.Title.value,
            From: queryForm.From.value,
            To: queryForm.To.value
        };
        return temp;
    }
    return {
        Init: function () {
            $('#sample_resourcetable').bootstrapTable({
                url: "/RS/Resource/GetResourceTaskList", //请求后台的URL（*）
                method: 'get',                              //请求方式（*）
                dataType: "json",                           //
                pagination: true,                           //是否显示分页（*）
                pageNumber: 1,                              //初始化加载第一页，默认第一页
                pageSize: 10,                               //每页的记录行数（*）
                pageList: [10, 25, 50, 100, 'All'],         //可供选择的每页的行数（*）
                cache: false,                               //是否使用缓存，默认为true（*）
                dataField: "rows",                         //
                queryParamsType: "limit",
                queryParams: queryParams,                   //传递参数（*）
                //showPaginationSwitch: true,
                sidePagination: "server",                   //分页方式：client客户端分页，server服务端分页（*）
                singleSelect: true,
                dataLocale: "zh-US",                        //表格汉化
                search: false,                              //是否显示表格搜索，此搜索是客户端搜索，不会进服务端
                clickToSelect: true,                        //是否启用点击选中行
                striped: true,                              //是否显示行间隔色
                uniqueId: "RequestID",                  //每一行的唯一标识，一般为主键列
                undefinedText: '-',
                height: 500,
                sortName: 'RequestDate',
                sortable: true,                            //是否启用排序
                sortOrder: 'desc',                           //排序方式
                columns: [
                    {
                        title: '标题',
                        field: 'Title',
                        align: 'center',
                        valign: 'left',
                        sortable: false
                    },
                    {
                        title: '描述',
                        field: 'Description',
                        align: 'center',
                        valign: 'middle',
                        sortable: false
                    },
                    {
                        title: '申请人',
                        field: 'RequestBy',
                        align: 'center',
                        valign: 'middle',
                        width: 100,
                        sortable: false
                    },
                    {
                        title: '申请时间',
                        field: 'RequestDate',
                        align: 'center',
                        valign: 'middle',
                        width: 120,
                        sortable: true,
                        formatter: function (value, row, index) {
                            return new RegExp(/^[0-9-]+/i).exec(value);
                        }
                    },
                    {
                        title: '截止时间',
                        field: 'EndDate',
                        align: 'center',
                        valign: 'middle',
                        width: 120,
                        sortable: true,
                        formatter: function (value, row, index) {
                            return new RegExp(/^[0-9-]+/i).exec(value);
                        }
                    },
                    {
                        title: '状态',
                        field: 'Status',
                        align: 'center',
                        valign: 'middle',
                        width: 200,
                        sortable: false,
                        formatter: function (value, row, index) {
                            var content = '';
                            if (value == 0) {
                                content = '<label class="status-progressbar" onclick="ShowRequestSchedule(' + row.RequestID + ',' + value + ')">' + '<label class="label-info" style="width:200px;">&nbsp;' + statusList[value] + '</label></label>';
                            }
                            else {
                                var w = value * (1 / 8) * 200;
                                var text = value < 3 ? '&nbsp;' : statusList[value] + '&nbsp;';
                                var outText = '<label style="font-size: 12px;">&nbsp;' + statusList[value] + '</label>';
                                var label = '<label class="label-info" style="width:' + w + 'px;"> ' + text + ' </label>';
                                content = '<label class="status-progressbar" onclick="ShowRequestSchedule(\'' + row.RequestID + '\')">' + (label + (value < 3 ? outText : '')) + '</label>';
                            }
                            return content;
                        }
                    },
                    {
                        title: '操作',
                        field: 'opr',
                        align: 'center',
                        valign: 'middle',
                        width: 120,
                        sortable: false,
                        formatter: function (value, row, index) {
                            var action = '';
                            if (row.CurrentExecutiveByID == $('#txtUserID').val()) {
                                switch (row.Status) {
                                    case 1:
                                        action = '<a onclick="ShowActionForm(2,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">签批</a>';
                                        break;       
                                    case 2:          
                                        action = '<a onclick="ShowActionForm(3,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">审批</a>';
                                        break;       
                                    case 3:          
                                        action = '<a onclick="ShowActionForm(4,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">授理</a>';
                                        break;       
                                    case 4:          
                                        action = '<a onclick="ShowActionForm(5,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">上传文件</a>';
                                        break;       
                                    case 5:          
                                        action = '<a onclick="ShowActionForm(6,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">初审</a>';
                                        break;       
                                    case 6:          
                                        action = '<a onclick="ShowActionForm(7,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">复审</a>';
                                        break;       
                                    case 7:          
                                        action = '<a onclick="ShowActionForm(8,' + row.RequestID + ',' + row.Status + ',' + row.UserType + ')">终审</a>';
                                        break;
                                }
                            }
                            // class="btn blue btn-outline btn-task"
                            if (row.Status >= 5) {
                                action += ' <a FileID="' + row.FileID + '" onclick="DownloadFile(this)">下载</a> '
                            }
                            return '<div class="btn-group inline">' + action + '</div>';
                        }
                    }
                ]
            });
            //$('#sample_editable_1').bootstrapTable({ height: 400 });
        },
        Refresh: function () {
            $('#sample_resourcetable').bootstrapTable(
                "refresh",
                {
                    url: "/RS/Resource/GetResourceTaskList",
                    queryParams: queryParams
                }
            );
        }
    }
};

var ScheduleTableInit = function () {
    _requestID = 0,
    _requestStatus = 0,
    queryParamsSchedule = function (params) {
        var temp = {
            RefID: _requestID,
            RefType: '资源申请业务流程'
        };
        return temp;
    }
    return {
        InitSchedule: function () {
            $.ajax({
                type: "GET",
                url: "/SYS/Workflow/GetProcessStepEntityList",
                data: {
                    RefID: _requestID,
                    RefType: '资源申请业务流程'
                },
                dataType: "json",
                success: function (data) {
                    if (data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            if (data[i]["IsFinish"] || _requestStatus == 0) {
                                $('.mt-step-col[step="' + data[i]["StepNum"] + '"]').addClass('done');
                            }
                            else if (data[i]["IsCurrent"]) {
                                $('.mt-step-col[step="' + data[i]["StepNum"] + '"]').addClass('error');
                            }
                        }
                    }
                },
                error: function (xhr, errorMsg, obj) {
                    bootbox.alert(xhr.responseText);
                }
            });
        },
        Init: function () {
            $('#sample_processlogtable').bootstrapTable({
                url: "/SYS/Workflow/GetProcessLogList", //请求后台的URL（*）
                //data: [{ "logID": 25, "ProcessEntityID": "be1f8bb1-8ea2-e711-bbae-000c29ae6fc6", "RefID": 13, "ExecutiveByID": 13, "ExecutiveBy": "系统管理员", "ExecutiveTime": "2017-09-26T15:46:16.037", "SubmitterID": 13, "SubmitBy": "系统管理员", "SubmitTime": "2017-09-26T15:46:16.037", "Content": "系统自动进行", "Conclusion": 1, "RefIDType": 0, "ProcessStepEntityID": "bf1f8bb1-8ea2-e711-bbae-000c29ae6fc6", "StepName": "提交请求", "StepNum": 1, "Status": "待签批" }],
                method: 'get',                              //请求方式（*）
                dataType: "json",                           //
                pagination: false,                           //是否显示分页（*）
                //pageNumber: 1,                              //初始化加载第一页，默认第一页
                //pageSize: 5,                               //每页的记录行数（*）
                //pageList: [5, 10, 25, 50, 100, 'All'],         //可供选择的每页的行数（*）
                cache: false,                               //是否使用缓存，默认为true（*）
                //dataField: "table",                         //
                queryParamsType: "limit",
                queryParams: queryParamsSchedule,                   //传递参数（*）
                //sidePagination: "client",                   //分页方式：client客户端分页，server服务端分页（*）
                singleSelect: true,
                dataLocale: "zh-US",                        //表格汉化
                search: false,                              //是否显示表格搜索，此搜索是客户端搜索，不会进服务端
                clickToSelect: true,                        //是否启用点击选中行
                striped: true,                              //是否显示行间隔色
                uniqueId: "logID",                      //每一行的唯一标识，一般为主键列
                undefinedText: '-',
                height: 280,
                sortName: 'ExecutiveTime',
                sortable: true,                            //是否启用排序
                sortOrder: 'desc',                           //排序方式
                columns: [
                    {
                        title: '操作',
                        field: 'StepName',
                        align: 'center',
                        valign: 'middle',
                        width: 120,
                        sortable: false
                    },
                    {
                        title: '提交人',
                        field: 'SubmitBy',
                        align: 'center',
                        valign: 'middle',
                        width: 100,
                        sortable: false
                    },
                    {
                        title: '审核人',
                        field: 'ExecutiveBy',
                        align: 'center',
                        valign: 'middle',
                        width: 100,
                        sortable: false
                    },
                    {
                        title: '审核时间',
                        field: 'ExecutiveTime',
                        align: 'center',
                        valign: 'middle',
                        sortable: false,
                        width: 150,
                        formatter: function (value, row, index) {
                            return value.replace(/T/, " ").replace(/\.[0-9]+/i, "");
                        }
                    },
                    {
                        title: '审核结果',
                        field: 'Conclusion',
                        align: 'center',
                        valign: 'middle',
                        sortable: false,
                        width: 80,
                        formatter: function (value, row, index) {
                            return value == 1 ? "通过" : "打回";
                        }
                    },
                    {
                        title: '审核意见',
                        field: 'Content',
                        align: 'left',
                        valign: 'middle',
                        sortable: false
                    }
                ]
            });
            //$('#sample_editable_1').bootstrapTable({ height: 400 });
        },
        Refresh: function () {
            $('#sample_processlogtable').bootstrapTable(
                "refresh",
                {
                    url: "/RS/Resource/GetProcessLogList",
                    queryParams: queryParamsSchedule
                }
            );
        },
        SetData: function (id, status) {
            _requestID = id;
            _requestStatus = status;
        }
    }
}

// 打开进度窗口
function ShowRequestSchedule(id, status) {
    oSchedule.SetData(id, status);
    $('#viewSchedule').modal('show');
}

// 打开操作窗口
function ShowActionForm(step, id, status, usertype) {
    switch (step) {
        case 2:
        case 3:
        case 4:
            // 签批，分发，分派
            step2Form.RequestID.value = id;
            step2Form.CurrentStep.value = step;
            step2Form.Status.value = status;
            $('#viewStep2').modal('show');
            break;
        case 5:
            // 上传资料
            uploadForm.RequestID.value = id;
            uploadForm.Status.value = status;
            $('#viewStepUpload').modal('show');
            break;
        case 6:
        case 7:
        case 8:
            // 初审，复审，终审
            step6Form.RequestID.value = id;
            step6Form.CurrentStep.value = step;
            step6Form.Status.value = status;
            step6Form.UserType.value = usertype;
            $('#viewStep6').modal('show');
            break;
    }
}

// 签批，分发，分派
function Allocate() {
    var data = {};
    data.RequestID = step2Form.RequestID.value;
    data.Status = step2Form.Status.value;
    data.Conclusion = step2Form.Conclusion.value;
    data.Con = step2Form.Content.value;
    if (data.Conclusion == 0) {
        data.NextUserID = 0;
        data.NextUserName = "";
    }
    else {
        data.NextUserID = $(step2Form.NextUser).find('option:selected').val();
        data.NextUserName = $(step2Form.NextUser).find('option:selected').text();
        if (data.NextUserName == '') {
            bootbox.alert('请选择下一步执行人');
            return;
        }
    }

    $.ajax({
        type: "POST",
        url: "/RS/Resource/ResourceRequestApprove",
        data: data,
        dataType: "json",
        success: function (data) {
            if (data['result'] == 'success') {
                $('#viewStep2').modal('hide');
                oTable.Refresh();
            }
            else {
                bootbox.alert(data['message']);
            }
        },
        error: function (xhr, errorMsg, obj) {
            bootbox.alert(xhr.responseText);
        }
    });
}

// 上传资料
function UploadFileAndSubmit() {
    $('#uploadForm').data('bootstrapValidator').validate();
    if (!$('#uploadForm').data('bootstrapValidator').isValid()) { return false; }

    $.ajax({
        url: '/RS/Resource/ResourceRequestUpload',
        type: 'POST',
        data: new FormData($("#uploadForm")[0]),
        async: false,
        cache: false,
        contentType: false,
        processData: false,
        dataType: 'json',
        success: function (data) {
            if (data.result == "success") {
                $("#viewStepUpload").modal("hide");
                oTable.Refresh();
            }
            else {
                bootbox.alert(data.message);
            }
        },
        error: function (data) {
            bootbox.alert(data);
        }
    });
}

// 初审，复审，终审
function Approve() {
    var data = {};
    data.RequestID = step6Form.RequestID.value;
    data.Status = step6Form.Status.value;
    data.Conclusion = step6Form.Conclusion.value;
    data.Con = step6Form.Content.value;

    if ($(step6Form.NextUser).is(':hidden')) {
        data.NextUserID = 0;
        data.NextUserName = "";
    }
    else {
        data.NextUserID = $(step6Form.NextUser).find('option:selected').val();
        data.NextUserName = $(step6Form.NextUser).find('option:selected').text();
        if (data.NextUserName == '') {
            bootbox.alert('请选择下一步执行人');
            return;
        }
    }

    $.ajax({
        type: "POST",
        url: "/RS/Resource/ResourceRequestApprove",
        data: data,
        dataType: "json",
        success: function (data) {
            if (data['result'] == 'success') {
                $('#viewStep6').modal('hide');
                oTable.Refresh();
            }
            else {
                bootbox.alert(data['message']);
            }
        },
        error: function (xhr, errorMsg, obj) {
            bootbox.alert(errorMsg);
        }
    });
}

// 下载文件
function DownloadFile(obj) {
    var form = $('<form method="post" action="/SYS/File/Download" target="_blank"></form>');
    form.append($('<input type="hidden" name="FileID" value="' + $(obj).attr('FileID') + '" />'));
    form.appendTo(document.body);
    form.submit();
    form.remove();
    //$('<form action="/SYS/File/Download" method="post" target="_blank"><input type="text" name="FileID" value="' + $(obj).attr('FileID') + '"/></form>').appendTo('body').submit().remove();
}

