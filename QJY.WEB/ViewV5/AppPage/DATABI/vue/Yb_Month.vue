
<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title">
            <el-input v-model="pzoption.value" style="display:none">
            </el-input>
            <el-date-picker v-model="childpro.chivalue"
                            type="monthrange"
                            align="right"
                            unlink-panels
                            :value-format="childpro.dateformat"
                            range-separator="-"
                            start-placeholder="开始月份"
                            end-placeholder="结束月份"
                            :picker-options="childpro.pickerOptionsd">
            </el-date-picker>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="默认当前月份">
                    <el-switch v-model="childpro.ishasdefault"></el-switch>
                </el-form-item>
            </el-form>
        </el-dialog>
    </el-col>
</template>
<script>
    module.exports = {
        data() {
            return {
                dialogInputVisible: false,
                childpro: {
                    placeholder: "占位符",
                    dateformat: "yyyy-MM-DD",
                    ishasdefault: true,
                    chivalue: [],
                    pickerOptionsd: {
                        shortcuts: [{
                            text: '本月',
                            onClick(picker) {
                                picker.$emit('pick', [new Date(), new Date()]);
                            }
                        }, {
                            text: '今年至今',
                            onClick(picker) {
                                const end = new Date();
                                const start = new Date(new Date().getFullYear(), 0);
                                picker.$emit('pick', [start, end]);
                            }
                        }, {
                            text: '最近六个月',
                            onClick(picker) {
                                const end = new Date();
                                const start = new Date();
                                start.setMonth(start.getMonth() - 6);
                                picker.$emit('pick', [start, end]);
                            }
                        }]
                    }

                }
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
            }
        },
        mounted: function () {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
                if (chi.pzoption.childpro.placeholder) {
                    chi.childpro = chi.pzoption.childpro;
                }
                if (chi.childpro.ishasdefault && app.isview) {
                    //浏览模式且有默认值的时候初始化
                    var nowdate = ComFunJS.getnowdate('yyyy-mm') + "-01";
                    chi.childpro.chivalue = [nowdate, nowdate];
                }
            })

        },
        watch: {
            "childpro.chivalue": {
                handler(newV, oldV) {
                    if (newV) {
                        var sdate = newV[0];
                        var edate = ComFunJS.format(ComFunJS.DateAdd(ComFunJS.StringToDate(newV[1]), 'm', 1), 'yyyy-MM-dd');
                        this.pzoption.value = sdate + "," + edate;
                    }

                },
                deep: true
            },
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            }
         



        }
    };
</script>