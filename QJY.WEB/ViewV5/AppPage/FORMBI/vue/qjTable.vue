<template>
    <el-col :sm="24" :md="24">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <p style="FONT-SIZE: 16PX; MARGIN-BOTTOM: 5PX;" v-text="pzoption.title"></p>
        <el-input v-model="pzoption.value" style="display:none">
        </el-input>
        <el-table :data="childpro.dataset" style="width: 100%" stripe border fit @header-dragend="widchange" :span-method="objectSpanMethod" :row-class-name="tableRowClassName" :summary-method="getSummaries" :show-summary="childpro.ishj">
            <el-table-column v-for="col in childpro.columdata" :label="col.colname" :key="col.colid" :width="col.width" min-width="120" align="center" :colid="col.colid" :show-overflow-tooltip="col.istip">
                <template slot-scope="scope">
                    <el-input v-if="col.coltype=='el-input'" v-model="scope.row[col.colid]" :disabled="childpro.disabled"></el-input>
                    <el-date-picker v-if="col.coltype=='el-date-picker'" value-format="yyyy-MM-dd" v-model="scope.row[col.colid]" :disabled="childpro.disabled"></el-date-picker>
                    <el-select v-if="col.coltype=='el-select'" v-model="scope.row[col.colid]" placeholder="请选择" :disabled="childpro.disabled">
                        <el-option v-for="item in col.options"
                                   :key="item.value"
                                   :label="item.label"
                                   :value="item.value">
                        </el-option>
                    </el-select>
                    <span v-if="col.coltype=='el-lable'">{{scope.row[col.colid]}}</span>
                </template>
            </el-table-column>
            <el-table-column type="index" width="40" fixed="left"> </el-table-column>
            <el-table-column label="操作" fixed="right" align="center" width="100" v-if="childpro.iskbj&&!childpro.disabled">
                <template slot-scope="scope">
                    <el-button @click.native.prevent="copRow(scope.$index,childpro.dataset)"
                               type="text"
                               size="small">
                        复制
                    </el-button>
                    <el-button @click.native.prevent="delRow(scope.$index,childpro.dataset)"
                               type="text"
                               size="small">
                        删除
                    </el-button>

                </template>
            </el-table-column>
        </el-table>
        <el-button v-if="childpro.iskbj&&!childpro.disabled" type="primary" size="mini" class="mt10 pull-right" @click="addRow">添加行<i class="el-icon-plus"></i></el-button>

        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-tabs type="border-card">
                <el-tab-pane label="个性配置">
                    <el-form :model="childpro">
                        <el-form-item label="必填">
                            <el-switch v-model="childpro.rules.required"></el-switch>
                        </el-form-item>
                        <el-form-item label="只读">
                            <el-switch v-model="childpro.disabled"></el-switch>
                        </el-form-item>
                        <el-form-item label="可编辑">
                            <el-switch v-model="childpro.iskbj"></el-switch>
                        </el-form-item>
                        <!--<el-form-item label="开启合计">
                            <el-switch v-model="childpro.ishj"></el-switch>
                        </el-form-item>-->
                    </el-form>
                </el-tab-pane>
                <el-tab-pane label="字段管理" v-model="childpro.tabname">

                    <el-tabs tab-position="left">
                        <el-tab-pane v-for="(el,cindex) in childpro.columdata" :label="el.colname" :name="el.colid" v-bind:key="el.colid" style="min-height: 250px;">

                            <table class="table table-bordered table-striped table-hover table-condensed">
                                <tr>
                                    <td style="text-align:right">列ID</td>
                                    <td v-text="el.colid"></td>
                                </tr>
                                <tr>
                                    <td style="text-align:right">列名称</td>
                                    <td>
                                        <el-input v-model="el.colname" size="mini"></el-input>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:right">列类型</td>
                                    <td>
                                        <el-radio-group v-model="el.coltype">
                                            <el-radio label="el-input">输入框</el-radio>
                                            <el-radio label="el-date-picker">日期</el-radio>
                                            <el-radio label="el-select">下拉框</el-radio>
                                            <el-radio label="el-lable">Lable占位符</el-radio>
                                        </el-radio-group>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:right">是否合计列</td>
                                    <td>
                                        <el-switch v-model="el.ishj"></el-switch>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:right">默认显示值</td>
                                    <td>
                                        <el-input v-model="el.defvalue" size="mini"></el-input>
                                    </td>
                                </tr>
                                <!--<tr>
                                    <td style="text-align:right">合并属性</td>
                                    <td>
                                        <el-input v-model="el.hbdata" size="mini"></el-input>
                                    </td>
                                </tr>-->
                                <tr>
                                    <td style="text-align:right">内容较多时是否启用tip</td>
                                    <td>
                                        <el-switch v-model="el.istip"></el-switch>
                                    </td>
                                </tr>




                                <tr v-if="el.coltype === 'el-select'">
                                    <td>下拉选项</td>
                                    <td>

                                        <el-card shadow="never">
                                            <el-tag :key="tag.value"
                                                    v-for=" (tag, index) in el.options"
                                                    closable
                                                    :disable-transitions="false"
                                                    @close="handleClose(tag,el)">
                                                {{tag.label}}
                                            </el-tag>
                                            <el-input class="input-new-tag"
                                                      v-if="inputVisible"
                                                      v-model="inputValue"
                                                      :ref="'saveTagInput'+cindex"
                                                      size="small"
                                                      @keyup.enter.native="handleInputConfirm(el)"
                                                      @blur="handleInputConfirm(el)">
                                            </el-input>
                                            <el-button v-else class="button-new-tag" size="small" @click="showInput(cindex)">+ New Item</el-button>
                                        </el-card>

                                    </td>

                                </tr>
                            </table>

                        </el-tab-pane>

                    </el-tabs>
                    <el-button type="primary" size="mini" class="mt10" @click="addcol()">添加列<i class="el-icon-plus"></i></el-button>

                </el-tab-pane>
            </el-tabs>
        </el-dialog>
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
                dialogInputVisible: false,
                inputVisible: false,
                inputValue: '',
                childpro: {
                    placeholder: "占位符",
                    disabled: false,
                    readonly:false,
                    itemtype: "text",
                    nowcell: { rowindex: "0", colindex: "0", rowspan: "1", colspan: "1" },
                    columdata: [],
                    dataset: [],
                    rowlen: 5,
                    iskbj: true,
                    tabname: "",
                    hbdata: [],
                    ishj: false,
                    dynamicTags: [],
                    rules: {
                        required: false, message: '不能为空', type: "string"
                    }
                }

            };
        },
        methods: {
            genID: function () {
                var random = Math.floor(Math.random() * (90000 - 10000 + 1) + 10000);
                return random;
            },
            tableRowClassName({ row, rowIndex }) {
                //把每一行的索引放进row
                row.index = rowIndex;
            },
            delWid: function (wigdetcode) {
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            },
            handleClose: function (tag,el) {
                _.remove(el.options, function (obj) {
                    return obj.value == tag.value;
                });
            },
            showInput: function (cindex) {
                this.inputVisible = true;
                this.$nextTick(_ => {
                    this.$refs["saveTagInput" + cindex][0].$refs.input.focus();
                });
            },
            handleInputConfirm: function (el) {
                let inputValue = this.inputValue;
                if (inputValue) {
                    el.options.push({ label: inputValue, value: inputValue });
                    this.inputVisible = false;
                    this.inputValue = '';
                }
           
            },

            addcol: function () {
                var colid = "col" + this.genID();
                var colindex = this.childpro.columdata.length + 1;
                this.childpro.columdata.push({ "colid": colid, "colname": "列" + colindex, "coltype": "el-input", "defvalue": "", "readonly": false, "field": "", "width": "", "ishj": false, "istip": true, options: [], hbdata: "" });
                this.childpro.tabname = colid;
            },
            delfiled: function (queryitem) {
                var index = _.findIndex(this.childpro.columdata, function (obj) {
                    return obj.colname == queryitem.colname;
                });
                this.childpro.columdata.splice(index, 1)

            },
            mangerel: function (components) {
                _.forEach(components, function (obj) {
                    debugger;
                    switch (obj.$attrs.coltype) {
                        case "el-input-number":
                            break;
                        case "month":
                            break;
                        case "el-select":
                            obj.$attrs.options = [{
                                value: '选项1',
                                label: '黄金糕'
                            }, {
                                value: '选项2',
                                label: '双皮奶'
                            }, {
                                value: '选项3',
                                label: '蚵仔煎'
                            }, {
                                value: '选项4',
                                label: '龙须面'
                            }, {
                                value: '选项5',
                                label: '北京烤鸭'
                            }]
                            break;
                        default:
                            return ""
                    }
                })
            },
            addRow: function () {
                var chi = this;
                var temp = {};
                _.forEach(chi.childpro.columdata, function (col) {
                    temp[col.colid] = col.defvalue;
                })
                chi.childpro.dataset.push(temp);

            },
            copRow: function (index, rows) {
                var temp = JSON.stringify(rows[index]);
                rows.splice(index, 0, JSON.parse(temp));

            },
            delRow: function (index, rows) {
                rows.splice(index, 1);
            },

            objectSpanMethod: function ({ row, column, rowIndex, columnIndex }) {
                if (column.hasOwnProperty("label")) {
                    try {
                        var temp = _.find(this.childpro.columdata, function (obj) {
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
                    var col = _.find(this.childpro.columdata, function (obj) {
                        return obj.colname == column.label && obj.ishj == true;
                    })
                    debugger;
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
                    if (wiget.wigdetcode == pro.childpro.wigdetcode) {
                        _.forEach(wiget.columdata, function (col) {
                            if (column.label == col.colname) {
                                col.width = newWidth;
                            }
                        })
                    }
                })

            }
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (this.pzoption.childpro.placeholder) {
                    this.childpro = this.pzoption.childpro
                }
            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    //if (this.childpro.dataset.length>0) {
                    //}
                    this.pzoption.value = JSON.stringify(this.childpro.dataset);
                    this.senddata();
                },
                deep: true
            }
        },
        created() {
            this.$nextTick(function () {
                // DOM 更新了
                // console.debug(this.$refs.refCol)
                // this.mangerel(this.$refs.refCol)
            })
        }
    };
</script>