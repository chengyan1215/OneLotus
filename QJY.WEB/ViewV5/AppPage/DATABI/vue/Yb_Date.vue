
<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title">
            <el-input v-model="pzoption.value" style="display:none">
            </el-input>
            <el-date-picker v-model="childpro.chivalue"
                            align="right"
                            unlink-panels
                            type="daterange"
                            :value-format="childpro.dateformat"
                            range-separator="至"
                            start-placeholder="开始日期"
                            end-placeholder="结束日期"
                            :picker-options="childpro.pickerOptionsd">
            </el-date-picker>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="默认当前时间">
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
                    ishasdefault: false,
                    chivalue: [],
                    pickerOptionsd: {
                        shortcuts: [{
                            text: '最近一周',
                            onClick(picker) {
                                const end = new Date();
                                const start = new Date();
                                start.setTime(start.getTime() - 3600 * 1000 * 24 * 7);
                                picker.$emit('pick', [start, end]);
                            }
                        }, {
                            text: '最近一个月',
                            onClick(picker) {
                                const end = new Date();
                                const start = new Date();
                                start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
                                picker.$emit('pick', [start, end]);
                            }
                        }, {
                            text: '最近三个月',
                            onClick(picker) {
                                const end = new Date();
                                const start = new Date();
                                start.setTime(start.getTime() - 3600 * 1000 * 24 * 90);
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
                    var nowdate = ComFunJS.getnowdate('yyyy-mm') + "-01";
                    chi.childpro.chivalue = [nowdate, nowdate];
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
            "childpro.chivalue": {
                handler(newV, oldV) {
                    var sdate = newV[0];
                    var edate = ComFunJS.format(ComFunJS.DateAdd(ComFunJS.StringToDate(newV[1]), 'd', 1), 'yyyy-MM-dd');
                    this.pzoption.value = sdate + "," + edate;
                },
                deep: true
            }
        }
    };
</script>