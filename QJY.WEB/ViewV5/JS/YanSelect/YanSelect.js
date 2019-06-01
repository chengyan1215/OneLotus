/*
* YanSelect 选择插件
* Copyright 2011, 刘玉宏
* 2011-05-10 v1.0	编写
*/
(function ($) {
    $.fn.YanSelect = function (options) {
        var defaults = {
            secrhindex: [0, 1, 2, 3, 4, 5],//搜索列
            valueindex: 0,//值对应列
            textindex: 1,//显示对应列
            columns: [],//列头数据
            isPage: false,//是否分页
            pagelen: 10,//页长度
            isMulSel: false,//是不是多选
            iscanedit: false,//是否能编辑
            dataobj: null,//数据源
            width: "400px",
            height: "400px",
            placetip: "搜索关键字",
            isCanAdd:true,
            afterSelect: $.noop,
            eventadd: $.noop//选中事件
        };
        var sel = 0;
        var options = $.extend(defaults, options);
        var TableHelp = {
            jsontotable: function ($tab, tabjson, cols) {

                //构造Header
                var TabHeader = "<thead><tr>";
                for (var i = 0; i < cols.length; i++) {
                    TabHeader = TabHeader + "<th>" + cols[i].text + "</th>";
                }
                TabHeader = TabHeader + "</tr></thead>";
                $tab.append(TabHeader);

                //构造BODY
                var TabBody = "<tbody class='tabData'>";
                for (var i = 0; i < tabjson.length; i++) {
                    TabBody = TabBody + TableHelp.addrow(tabjson[i]);
                }
                TabBody = TabBody + "</tbody><tfoot style='display:none'><tr><td  colspan='" + cols.length + "'>没有找到匹配的数据</td></tr></tfoot>";
                return $tab.append(TabBody);
            },
            addrow: function (rowdata) {
                cols = options.columns;
                var TabTr = "<tr class='datatr' dataid='" + rowdata["ID"] + "'>";
                for (var m = 0; m < cols.length; m++) {
                    var FieldName = cols[m].fieldname;
                    var tdvalue = rowdata[FieldName];//默认为dataField对应值
                    //处理width属性
                    var width = "";
                    if (cols[m].hasOwnProperty('width')) {
                        width = "width:" + cols[m]['width'];
                    }
                    //处理width属性
                    if (cols[m].hasOwnProperty('ftcustom')) {
                        if ($.isFunction(cols[m].ftcustom)) {
                            tdvalue = "";
                            tdvalue = cols[m].ftcustom(rowdata, i);
                        }
                    }
                    if (cols[m].hasOwnProperty('type')) {
                        if (cols[m]['type'] == 'date') {
                            tdvalue = tdvalue.replace("T", " ");
                        }
                    }
                    if (tdvalue == null) {
                        tdvalue = "";
                    }
                    TabTr = TabTr + "<td style='" + width + "'>" + tdvalue + "</td>";
                }
                TabTr = TabTr + "</tr>";
                return TabTr;
            },
            page: function (tab) {
                //添加分页
                var pagelen = Math.ceil($("tbody tr[yanissel='Y']", tab).length / options.pagelen);
                var pagestr = "<tfoot><tr ><td style='text-align: center;' colspan=" + $("th", tab).length + "><div class='pagination' style='margin:0px' ><ul><li class='previous disabled'><a href='#'>«</a></li>";
                if (pagelen === 0) {
                    pagestr = pagestr + "<li ><a href='#'>暂无数据</a></li>";
                }
                else {
                    for (var i = 1; i < pagelen + 1; i++) {
                        pagestr = pagestr + "<li class='" + (i === 1 ? 'page active' : 'page') + "' ><a href='#'>" + i + "</a></li>";
                    }
                }
                pagestr = pagestr + "<li class='next'><a href='#'>»</a></li></ul></div></td></tr></tfoot>";
                tab.find("tfoot").remove().end().append(pagestr);
                //添加分页

                //默认长度
                $("tbody tr", tab).slice(options.pagelen).hide();

                //绑定事件(页数)

                $("li", tab).click(function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    var pageindex = 1;
                    //当前页
                    if ($(this).hasClass("page")) {
                        pageindex = $(this).text();
                    }
                    //上一页
                    if ($(this).hasClass("previous")) {
                        pageindex = $(".active", tab).text() - 1;
                    }
                    //下一页
                    if ($(this).hasClass("next")) {
                        pageindex = $(".active", tab).text() * 1 + 1
                    }
                    if (pageindex > 0 && pageindex <= pagelen) {
                        TableHelp.gopage(tab, pageindex);
                        $("li", tab).removeClass("disabled active");
                        $("li", tab).eq(pageindex).addClass("disabled active");
                    }
                })
            },
            gopage: function (tab, pageindex) {

                $("tbody tr", tab).slice(0, (pageindex - 1) * options.pagelen).each(function () {
                    if ($(this).attr("yanissel") === 'Y') {
                        $(this).hide();
                    }
                });
                $("tbody tr", tab).slice((pageindex - 1) * options.pagelen, pageindex * options.pagelen).each(function () {
                    if ($(this).attr("yanissel") === 'Y') {
                        $(this).show();
                    }
                })
                $("tbody tr", tab).slice(pageindex * options.pagelen).each(function () {
                    if ($(this).attr("yanissel") === 'Y') {
                        $(this).hide();
                    }
                })

            },
            extaddrow: function (rowdata) {
                $(".tabData").append(TableHelp.addrow(rowdata));
            }
        };
        this.each(function () {
            var txt = $(this);
            if (!txt.attr("isinit")) {
                //如果已经有值并且没有实际值
                if (txt.val() && !txt.attr("realvalue")) {
                    for (var i = 0; i < options.dataobj.length; i++) {
                        if (options.dataobj[i].ID == txt.val()) {
                            var FieldName = options.columns[options.textindex].fieldname;
                            txt.val(options.dataobj[i][FieldName]).attr("realvalue")
                        }
                    }
                }
                txt.attr("autocomplete", "off").attr("isinit", "Y");
                var $tab = $("<table></table>", { style: "width:" + options.width }).addClass("table table-striped table-bordered table-condensed table-hover");
                TableHelp.jsontotable($tab, options.dataobj, options.columns);
                var tablediv = $("<div>", { "class": "YanTabDiv " }).append($tab.show()).hide()
                $("body").append(tablediv);
                if (!options.iscanedit) {
                    txt.attr("onfocus", "this.blur()");
                    $("thead", $tab).prepend("<tr><th style='text-align:left' colspan='" + options.columns.length + "'>  <div class='input-group' style='width:100%'> <input type='text' class='searinput form-control' placeholder='" + options.placetip + "' /><span class='input-group-addon YanSelect_Add' >新增</span></div> </th></tr>");
                    //新增回调
                    if (options.isCanAdd) {
                        $(".YanSelect_Add", $tab).click(function () {
                            if ($.isFunction(options.eventadd)) {
                                options.eventadd.call(this, $(".searinput", $tab).val(), $(".YanSelect_Add", $tab));
                            }
                        })
                    } else {
                        $(".YanSelect_Add", $tab).remove();
                    }
                   


                }
                //选择值
                $(".datatr").live("click", function (event) {
                    event.stopPropagation();
                    event.preventDefault();
                    setvalue($(this));
                })
                //txt事件
                var searchinput = $(".searinput", $tab);
                txt.click(function (e) {
                    $(".YanTabDiv").hide().find(".searinput").val("");
                    var txtoffset = $(this).offset();
                    var txth = $(this).outerHeight(true);
                    tablediv.css({ "top": txtoffset.top + txth, "left": txtoffset.left }).show();
                    //获取数据
                    getdata(txt);
                    $(".searinput", tablediv).focus().trigger('keyup');

                })
                txt.add(searchinput).keyup(function (e) {
                    if (e.keyCode == 13) {//enter
                        e.preventDefault();
                        $("tbody tr", tablediv).each(function () {
                            if ($(this).hasClass("success")) {
                                setvalue($(this));
                                $("tbody tr", tablediv).removeClass("success")
                            }
                        })
                    } else if (e.keyCode == 37) {//left
                    } else if (e.keyCode == 38) {//up
                        if (sel > 0) {
                            sel--;
                            $("tbody tr", tablediv).removeClass("success").eq(sel).addClass("success");
                        }
                    } else if (e.keyCode == 39) {//right
                    } else if (e.keyCode == 40) {//down
                        //计算向下键能移动的距离
                        var count = options.pagelen > $("tbody tr[yanissel='Y']", tablediv).length ? $("tbody tr[yanissel='Y']", tablediv).length : options.pagelen;
                        if (sel < count) {
                            $("tbody tr", tablediv).removeClass("success").eq(sel).addClass("success");
                            sel++;
                        }
                    } else {
                        sel = 0;
                        tablediv.show();
                        if (!options.iscanedit) {
                            getdata(searchinput);
                        }
                        else {
                            getdata(txt);
                        }
                    }
                });
                if (options.isMulSel) {
                    txt.blur(function () {
                        var realvalue = "";
                        txt.val(txt.val().substr(0, txt.val().lastIndexOf(",") + 1))
                        var arr = txt.val().substr(0, txt.val().length - 1).split(',');
                        for (var i = 0; i < arr.length; i++) {
                            $("tbody tr", tablediv).each(function () {
                                if ($(this).find('td').eq(options.textindex).text() == arr[i]) {
                                    realvalue = realvalue + $(this).find('td').eq(options.valueindex).text() + ",";
                                }
                            });
                        }
                        txt.attr("realvalue", realvalue);
                    });
                }
                var getdata = function (domint, istext) {
                    var text = domint.val();
                    if (text != "") {
                        $("tbody tr", tablediv).each(function () {
                            var trtext = "";
                            var tr = $(this);
                            $.each(options.secrhindex, function (i, n) {
                                if (tr.find('td').eq(n)) {
                                    trtext = trtext + $.trim(tr.find('td').eq(n).text());
                                }
                            });
                            if (trtext.indexOf(text) >= 0) {
                                tr.show().attr("yanissel", "Y");
                            }
                            else {
                                tr.hide().attr("yanissel", "N");
                            }
                        })
                    }
                    else {
                        $("tbody tr", tablediv).attr("yanissel", "Y").show();
                    }
                    //如果不分页，则设定高度
                    if (options.isPage) {
                        TableHelp.page($tab);
                    } else {
                        tablediv.css({ "max-height": options.height, "overflow-y": "auto" });

                    }
                    $("tbody tr", tablediv).removeClass("success");



                    if ($("tbody tr:visible", tablediv).length == 0) {
                        $("tfoot", tablediv).show()
                    } else {
                        $("tfoot", tablediv).hide()
                    }
                }

                var setvalue = function (jdom) {
                    var text = $.trim(jdom.find('td').eq(options.textindex).text());
                    var value = $.trim(jdom.find('td').eq(options.valueindex).text());
                    if (options.isMulSel) {
                        txt.val(txt.val().substr(0, txt.val().lastIndexOf(",") + 1) + text + ",");
                    }
                    else {
                        txt.val(text).attr("realvalue", value);
                        tablediv.hide();

                    }
                    txt.trigger("blur").trigger("change").next('input:hidden').val(value).trigger("change");
                    //回调
                    if ($.isFunction(options.afterSelect)) {
                        options.afterSelect.call(this, jdom);
                    }
                }
                $("body").click(function (event) {
                    var eventsrc = event.srcElement || event.target;
                    if (eventsrc) {
                        if (!tablediv[0].contains(eventsrc) && !txt[0].contains(eventsrc)) {
                            tablediv.hide();
                        }
                        sel = 0;
                    }
                });
            }

        });
        return TableHelp;
    };
})(jQuery);