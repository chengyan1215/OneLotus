<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" style="    line-height: 40px;" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <div style="width:100%;height:40px; padding-left:10px; line-height:40px; border: 1px solid #EBEEF5;border-bottom: 0px;">
            {{pzoption.title}}  <el-button size="mini" style="float:right;margin-top: 5px;margin-right:10px" @click="exportxls()">导出Excel</el-button>
        </div>
        <el-table :data="pzoption.dataset" style="width: 100%" v-if="pzoption.datatype=='0'" stripe border fit @header-dragend="widchange" :row-class-name="tableRowClassName" :summary-method="getSummaries" :show-summary="pzoption.ishj" :height="pzoption.wigheight">
            <el-table-column v-for="col in pzoption.wdlist"  :prop="col.colid" :label="col.colname" :key="col.colid" :width="col.width" min-width="120" align="center" :colid="col.colid" :show-overflow-tooltip="col.istip" sortable>
                <template slot-scope="scope">
                    <span>{{mang(scope.row[col.colid],col)}}</span>
                </template>
            </el-table-column>



            <el-table-column v-for="col in pzoption.dllist"  :prop="col.colid"  :label="col.colname" :key="col.colid" :width="col.width" min-width="120" align="center" :colid="col.colid" :show-overflow-tooltip="col.istip" sortable>
                <template slot-scope="scope">
                    <span>{{mang(scope.row[col.colid],col)}}</span>
                </template>
            </el-table-column>
            <el-table-column type="index" width="50" fixed="left"> </el-table-column>

        </el-table>
        <el-table :data="pzoption.dataset" style="width: 100%" v-if="pzoption.datatype=='1'" stripe border fit @header-dragend="widchange" :row-class-name="tableRowClassName" :summary-method="getSummaries" :show-summary="pzoption.ishj" :height="pzoption.wigheight">
            <el-table-column v-for="col in pzoption.apicols" :prop="col.ColumnName" :label="col.ColumnName" :key="col.ColumnName" min-width="120" align="center" :colid="col.ColumnName" :show-overflow-tooltip="col.istip" sortable>

            </el-table-column>
            <el-table-column type="index" width="50" fixed="left"> </el-table-column>

        </el-table>
    </el-col>

</template>
<style>
</style>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data() {
            return {
                dialogFormVisible: false,
                nowcell: { rowindex: "0", colindex: "0", rowspan: "1", colspan: "1" },
                ysdata: []
            };
        },
        methods: {
            tableRowClassName({ row, rowIndex }) {
                //把每一行的索引放进row
                row.index = rowIndex;
            },
            mang: function (val, col) {
                var ysval = val;
                _.forEach(col.mapdata, function (obj) {
                    if (val == obj.val) {
                        ysval= obj.ysval;
                    }
                })
                return ysval;
            },
            delWid: function (wigdetcode) {
                this.$root.nowwidget = { rules: { required: false, message: '请填写信息', trigger: 'blur' } };
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            exportxls: function () {
                debugger;
                var title = [];
                _.forEach(this.pzoption.wdlist, function (obj) {
                    title.push({ "value": obj.colname, "type": "ROW_HEADER_HEADER", "datatype": "string", "colid": obj.colid })
                })
                _.forEach(this.pzoption.dllist, function (obj) {
                    title.push({ "value": obj.colname, "type": "ROW_HEADER_HEADER", "datatype": "string", "colid": obj.colid })
                })
                this.JSONToExcelConvertor(this.pzoption.dataset, "20190101", title);
            },
            JSONToExcelConvertor: function (JSONData, FileName, ShowLabel) {

                var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;

                var excel = '<table>';

                //设置表头
                var row = "<tr>";
                for (var i = 0, l = ShowLabel.length; i < l; i++) {
                    row += "<td>" + ShowLabel[i].value + '</td>';
                }

                //换行
                excel += row + "</tr>";

                //设置数据
                for (var i = 0; i < arrData.length; i++) {
                    var row = "<tr>";

                    for (var j = 0; j < ShowLabel.length; j++) {
                        var value = arrData[i][ShowLabel[j].colid];
                        row += '<td>' + value + '</td>';
                    }

                    excel += row + "</tr>";
                }

                excel += "</table>";

                var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>";
                excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">';
                excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel';
                excelFile += '; charset=UTF-8">';
                excelFile += "<head>";
                excelFile += "<!--[if gte mso 9]>";
                excelFile += "<xml>";
                excelFile += "<x:ExcelWorkbook>";
                excelFile += "<x:ExcelWorksheets>";
                excelFile += "<x:ExcelWorksheet>";
                excelFile += "<x:Name>";
                excelFile += "{worksheet}";
                excelFile += "</x:Name>";
                excelFile += "<x:WorksheetOptions>";
                excelFile += "<x:DisplayGridlines/>";
                excelFile += "</x:WorksheetOptions>";
                excelFile += "</x:ExcelWorksheet>";
                excelFile += "</x:ExcelWorksheets>";
                excelFile += "</x:ExcelWorkbook>";
                excelFile += "</xml>";
                excelFile += "<![endif]-->";
                excelFile += "</head>";
                excelFile += "<body>";
                excelFile += excel;
                excelFile += "</body>";
                excelFile += "</html>";


                var uri = 'data:application/vnd.ms-excel;charset=utf-8,' + encodeURIComponent(excelFile);

                var link = document.createElement("a");
                link.href = uri;

                link.style = "visibility:hidden";
                link.download = FileName + ".xls";

                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            },
            objectSpanMethod: function ({ row, column, rowIndex, columnIndex }) {
                if (column.hasOwnProperty("label")) {
                    try {
                        var temparr = [];
                        temparr = _.concat(temparr, this.pzoption.wdlist, this.pzoption.dllist);
                        var temp = _.find(temparr, function (obj) {
                            return obj.colname == column.label
                        }).hbdata;
                        var hbs = temp.split(",");
                        for (var i = 0; i < hbs.length; i++) {
                            var hb = hbs[i].split('-');
                            if (columnIndex == hb[0]) {
                                if (rowIndex == hb[1]) {
                                    return {
                                        rowspan: hb[2],
                                        colspan: 1
                                    };
                                }
                                var tempar = [];
                                for (var m = 1; m < hb[2] * 1; m++) {
                                    tempar.push(hb[1] * 1 + m);
                                }
                                if (tempar.indexOf(rowIndex) > -1) {
                                    return {
                                        rowspan: 0,
                                        colspan: 0
                                    };
                                }
                            }
                        }

                    } catch (e) {

                    }

                }


            },
            getSummaries: function (param) {

                const { columns, data } = param;
                const sums = [];

                columns.forEach((column, index) => {
                    if (index === 0) {
                        sums[index] = '合计';
                        return;
                    }

                    var temparr = [];
                    temparr = _.concat(temparr, this.pzoption.wdlist, this.pzoption.dllist);

                    var col = _.find(temparr, function (obj) {
                        return obj.colname == column.label && obj.ishj == true;
                    })

                    if (col) {
                        var temphj = 0;
                        _.forEach(data, function (obj) {
                            if (obj[col.colid]) {
                                temphj = temphj * 1 + obj[col.colid] * 1;
                            }
                        })
                        sums[index] = temphj;
                    } else {
                        sums[index] = "";
                    }

                });

                return sums;
                //const { columns, data } = param;
                //const sums = [];

                //columns.forEach((column, index) => {

                //if (index === 0) {
                //    sums[index] = '合计';
                //    return;
                //} else if (index == 4) {
                //    var col = _.find(this.pzoption.columdata, function (obj) {
                //        return obj.colname == column.label
                //    })
                //    debugger;
                //    var temphj = 0;
                //    _.forEach(data, function (obj) {
                //        if (obj[col.colid]) {
                //            temphj = temphj * 1 + obj[col.colid] * 1;
                //        }
                //    })
                //    sums[index] = temphj;
                //} else {
                //    sums[index] = "N/A";

                //}

                //});

                //return sums;

            },
            widchange: function (newWidth, oldWidth, column, event) {
                var pro = this;
                _.forEach(this.$root.FormData.wigetitems, function (wiget) {
                    var temp = {};
                    if (wiget.wigdetcode == pro.pzoption.wigdetcode) {
                        _.forEach(wiget.wdlist, function (col) {
                            if (column.label == col.colname) {
                                col.width = newWidth;
                            }
                        })
                        _.forEach(wiget.dllist, function (col) {
                            if (column.label == col.colname) {
                                col.width = newWidth;
                            }
                        })
                    }
                })

            }
        },
        created() {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
                chi.pzoption.mdwidth = 24;
                // DOM 更新了
                // console.debug(this.$refs.refCol)
                // this.mangerel(this.$refs.refCol)
            })
        }
    };
</script>