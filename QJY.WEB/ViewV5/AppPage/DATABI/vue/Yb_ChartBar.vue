<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <div class="qjchart" :id="'chat'+pzoption.wigdetcode" style="width:100%;" v-bind:style="{ height: pzoption.wigheight + 'px' }">图标</div>
    </el-col>

</template>
<script>
    module.exports = {
        props: {
            index: Number,
            pzoption: Object
        },
        data() {
            return {
                chartoption: {
                    title: {
                        text: '柱图',
                        left: 'center'
                    },
                    legend: {
                        bottom: 10,
                        left: 'center',
                        show: false
                    },
                    tooltip: {},
                    dataset: {
                        source: [
                         
                        ]
                    },
                    xAxis: {
                        type: 'category',
                        show: true,
                        axisLabel: {
                            interval: 0,
                            rotate:-45
                        }
                    },
                    yAxis: {
                        show: true
                    },
                    // Declare several bar series, each will be mapped
                    // to a column of dataset.source by default.
                    series: [
                        {
                            type: 'bar',
                            radius: '55%',
                            label: {
                                show: true
                            }
                        }
                    ]
                }
            };
        },
        methods: {
            delWid: function (wigdetcode) {
                this.$root.nowwidget = { rules: { required: false, message: '请填写信息', trigger: 'blur' } };//没这个删除不掉啊
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            resize: function () {
                var vtype = this.$root.vtype;
                var widall = $("#conwig").width();
                var wigwidth = widall;
                if (ComFunJS.isPC()) {//pc端需要初始化宽度
                    wigwidth = _.parseInt(widall / 24) * this.pzoption.mdwidth;
                }
                var wigheight = this.pzoption.tabheight;
                var chartdom = document.getElementById("chat" + this.pzoption.wigdetcode);
                echarts.getInstanceByDom(chartdom).resize({
                    width: wigwidth,
                    height: wigheight
                });
            }
        },
        mounted: function () {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
                var chatid = "chat" + chi.pzoption.wigdetcode;
                var myChart = echarts.init(document.getElementById(chatid));
                myChart.setOption(chi.chartoption);
            })
        },
        watch: {
            pzoption: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.chartoption.dataset.source = this.pzoption.dataset;
                    this.chartoption.title.text = this.pzoption.title;

                    var myChart = null;
                    var chartdom = document.getElementById("chat" + this.pzoption.wigdetcode);
                    var echartdom = echarts.getInstanceByDom(chartdom);
                    if (echartdom != undefined) {
                        myChart = echartdom;
                        this.resize();
                    } else {
                        myChart = echarts.init(chartdom);
                    }
                    myChart.setOption(this.chartoption);
                },
                deep: true
            }
        }

    };
</script>