
<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'"  :rules="childpro.rules">
            <el-date-picker align="right" :type="childpro.itemtype"   @change="getTime" :placeholder="childpro.placeholder" :picker-options="pickerOptions" :value-format="childpro.dateformat" :disabled="childpro.disabled" :readonly="childpro.readonly" v-model="pzoption.value" >
            </el-date-picker>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="日期类型">
                    <el-radio v-model="childpro.itemtype" label="year">年</el-radio>
                    <el-radio v-model="childpro.itemtype" label="month">月</el-radio>
                    <el-radio v-model="childpro.itemtype" label="date">日</el-radio>
                    <el-radio v-model="childpro.itemtype" label="datetime">时间</el-radio>
                </el-form-item>
                <el-form-item label="占位符">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="必填">
                    <el-switch v-model="childpro.rules.required"></el-switch>
                </el-form-item>
                <el-form-item label="只读">
                    <el-switch v-model="childpro.readonly"></el-switch>
                </el-form-item>
                <el-form-item label="默认当前时间">
                    <el-switch v-model="childpro.ishasdefault"></el-switch>
                </el-form-item>
            </el-form>
        </el-dialog>
    </el-col>
</template>
<script>
    // import { cloneDeepWith } from "lodash";
    module.exports = {
        data() {
            return {
                dialogInputVisible: false,
                childpro: {
                    placeholder: "占位符",
                    disabled: false,
                    readonly:false,
                    itemtype: "date",
                    dateformat: "yyyy-MM-dd",
                    ishasdefault: false,
                    rules: {
                        required: false, message: '不能为空', type: 'string', trigger: 'change'
                    }
                },
                pickerOptions: {
                    //disabledDate(time) {
                    //    return time.getTime() > Date.now();
                    //},
                    //shortcuts: [{
                    //    text: '今天',
                    //    onClick(picker) {
                    //        picker.$emit('pick', new Date());
                    //    }
                    //}, {
                    //    text: '昨天',
                    //    onClick(picker) {
                    //        const date = new Date();
                    //        date.setTime(date.getTime() - 3600 * 1000 * 24);
                    //        picker.$emit('pick', date);
                    //    }
                    //}, {
                    //    text: '一周前',
                    //    onClick(picker) {
                    //        const date = new Date();
                    //        date.setTime(date.getTime() - 3600 * 1000 * 24 * 7);
                    //        picker.$emit('pick', date);
                    //    }
                    //}]
                },

            };
        },
        props: ['pzoption', 'index'],
        methods: {
            delWid: function (wigdetcode) {
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            },
            getTime(val) {

            }
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.pzoption.childpro.placeholder) {
                    pro.childpro = pro.pzoption.childpro;
                    if (pro.childpro.ishasdefault && app.isview) {
                        pro.pzoption.value = ComFunJS.getnowdate(pro.childpro.dateformat)
                    }
                } else {
                    //首次的时候赋予默认值
                    this.pzoption.childpro = this.childpro;
                }
            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            },
            "childpro.itemtype": { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    switch (newV) {
                        case "year":
                            this.childpro.dateformat = "yyyy";
                            break;
                        case "month":
                            this.childpro.dateformat = "yyyy-MM";
                            break;
                        case "date":
                            this.childpro.dateformat = "yyyy-MM-dd";
                            break;
                        case "datetime":
                            this.childpro.dateformat = "yyyy-MM-dd HH:mm";
                            break;
                        default:
                            this.childpro.dateformat = "yyyy-MM-dd";
                    }
                },
                deep: true
            }
        }
    };
</script>