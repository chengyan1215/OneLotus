<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title">

            <el-select v-model="pzoption.value" style="width:100%" :disabled="childpro.readonly" multiple filterable clearable>
                <el-option v-for="item in childpro.dataset"
                           :key="item.value"
                           :label="item.label"
                           :value="item.value">
                </el-option>
            </el-select>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible" width="30%">
            <el-tabs type="border-card">

                <el-tab-pane label="数据配置" style="min-height: 420px;">
                    <el-radio v-model="childpro.datatype" label="0" style="display: block;">自定义数据项</el-radio>
                    <el-radio v-model="childpro.datatype" label="1" style="display: block;margin-left: 0;margin-top:5px">动态数据项</el-radio>
                    <el-radio v-model="childpro.datatype" label="2" style="display: block;margin-left: 0;margin-top:5px">关联表单</el-radio>

                    <div class="panel panel-default" style="border: 1px solid #ddd;margin-top:10px" v-if="childpro.datatype === '0'">

                        <div class="panel-heading">下拉选项</div>
                        <div class="panel-body">


                            <el-tag style="margin-top:5px"
                                    v-for="(tag, index1) in childpro.dynamicTags" v-bind:key="index1"
                                    closable
                                    :disable-transitions="false"
                                    @close="handleClose(tag)">
                                {{tag}}
                            </el-tag>
                            <el-input class="input-new-tag"
                                      v-if="inputVisible"
                                      v-model="inputValue"
                                      ref="saveTagInput"
                                      size="small"
                                      @keyup.enter.native="handleInputConfirm"
                                      @blur="handleInputConfirm">
                            </el-input>
                            <el-button style="margin-top:5px" v-else class="button-new-tag" size="small" @click="showInput">+ New Item</el-button>
                        </div>

                    </div>
                    <div class="panel panel-default" style="border: 1px solid #ddd;margin-top:10px" v-if="childpro.datatype === '1'">
                        <div class="panel-heading">动态SQL语句</div>
                        <div class="panel-body">
                            <el-input type="textarea"
                                      :rows="4"
                                      placeholder="请输入SQL语句"
                                      v-model="childpro.datasql">
                            </el-input>
                            <el-button plain class="mt10" @click="VailSql">验证</el-button>



                        </div>
                    </div>
                    <div class="panel panel-default" style="border: 1px solid #ddd;margin-top:10px" v-if="childpro.datatype === '2'">
                        <div class="panel-heading">关联表单</div>
                        <div class="panel-body">
                            <el-cascader :options="glformdata"
                                         v-model="childpro.glfiled"
                                         @change="handleChange"
                                         filterable>
                            </el-cascader>
                        </div>
                    </div>
                </el-tab-pane>

            </el-tabs>
        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data: function () {
            return {
                dialogInputVisible: false,
                inputVisible: false,
                inputValue: '',
                glformdata: [],
                childpro: {
                    placeholder: "请选择",
                    readonly: false,
                    datatype: "0",
                    datasql: "",
                    multiple: false,
                    dataset: [],
                    glfiled: [],
                    dynamicTags: []
                }
            }
        },
        methods: {
            delWid: function (wigdetcode) {
                // 子组件中触发父组件方法ee并传值cc12345
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            },
            VailSql: function () {
                var chi = this;
                $.getJSON("/API/VIEWAPI.ashx", { Action: "FORMBI_GETSQLDATA", P1: chi.childpro.datasql }, function (result) {
                    if (!result.ErrorMsg) {
                        app.$notify({
                            title: '成功',
                            message: '加载数据成功',
                            type: 'success'
                        });
                        if (result.Result.length > 0) {
                            var tempkeys = _.keys(result.Result[0]);
                            var temparr = [];
                            _.forEach(result.Result, function (obj) {
                                var tempval = obj[tempkeys[0]];
                                var templab = obj[tempkeys[0]];
                                if (tempkeys.length > 1) {
                                    templab = obj[tempkeys[1]];
                                }
                                temparr.push({ label: templab, value: tempval })
                            });
                            chi.childpro.dataset = temparr;
                        }


                    }
                })
            },
            handleChange: function (arrval) {
                var app = this;
                $.getJSON("/API/VIEWAPI.ashx", { Action: "FORMBI_GETBDTJDATA", P1: arrval[0], pdfields: arrval[1] }, function (result) {
                    if (!result.ErrorMsg) {
                        var temparr = [];
                        _.forEach(result.Result, function (obj) {
                            var tempval = obj[arrval[1]];
                            var templab = obj[arrval[1]];
                            if (_.findIndex(temparr, function (item) { return item.value == tempval }) < 0) {
                                temparr.push({ label: templab, value: tempval });
                            }
                        });
                        app.childpro.dataset = temparr;


                    }
                })
            },
            handleClose: function (tag) {
                this.childpro.dynamicTags.splice(this.childpro.dynamicTags.indexOf(tag), 1);
            },
            showInput: function () {
                this.inputVisible = true;
                this.$nextTick(_ => {
                    this.$refs.saveTagInput.$refs.input.focus();
                });
            },
            handleInputConfirm: function () {
                let inputValue = this.inputValue;
                if (inputValue) {
                    this.childpro.dynamicTags.push(inputValue);
                }
                this.inputVisible = false;
                this.inputValue = '';
            },
            ChandleClose: function (tag, el) {
                el.options.splice(el.options.indexOf(tag), 1);
            },
            CshowInput: function (index) {
                this.inputVisible = true;
                this.$nextTick(_ => {
                    this.$refs.CsaveTagInput[index].focus()
                });
            },
        },
        mounted: function () {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
                //获取关联字段可用的表单数据
                $.getJSON("/API/VIEWAPI.ashx", { Action: "FORMBI_GETFORMFILEDS" }, function (result) {
                    if (!result.ErrorMsg) {
                        chi.glformdata = result.Result;
                        if (chi.pzoption.childpro.placeholder) {
                            chi.childpro = chi.pzoption.childpro;
                            if (chi.childpro.datatype == '1') {
                                chi.VailSql();
                            }
                        }
                    }
                })

            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            },
            required: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    // this.childpro.rules.required = newV;
                }
            },
            'childpro.dynamicTags': {
                handler(newVal, oldVal) {
                    var temparr = [];
                    _.forEach(newVal, function (obj) {
                        temparr.push({ label: obj, value: obj })
                    });
                    this.childpro.dataset = temparr;
                },
                deep: true //对象内部属性的监听，关键。
            }
        }
    };
</script>