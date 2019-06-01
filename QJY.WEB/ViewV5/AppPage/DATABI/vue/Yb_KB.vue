<template>
    <el-col :sm="24" :md="pzoption.mdwidth" v-bind:style="{ height: pzoption.wigheight + 'px' }" style="overflow: auto; overflow-x: hidden;">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <p style="FONT-SIZE: 16PX; MARGIN-BOTTOM: 5PX;height:20px" v-text="pzoption.title"></p>
        <el-row :gutter="12" v-if="pzoption.datatype=='0'">
            <el-col :xs="12" :sm="12" :md="6" v-for="item in pzoption.dataset" class="mt10"  v-if="pzoption.wdlist.length>0">
                <el-card shadow="hover">
                    <div style="font-size: 32px;color: #0498ff;">{{item[pzoption.dllist[0].colid]}}<span style="font-size: 14px;color: #0498ff;"> {{childpro.dwname}}</span></div>
                    <div style="font-size:14px"> {{item[pzoption.wdlist[0].colid]}}</div>
                </el-card>
             
            </el-col>
            <el-col :xs="24" :sm="24" :md="24" v-for="item in pzoption.dataset" class="mt10"  v-if="pzoption.wdlist.length==0">
                <el-card shadow="hover">
                    <div style="font-size: 32px;color: #0498ff;">{{item[pzoption.dllist[0].colid]}}<span style="font-size: 14px;color: #0498ff;"> {{childpro.dwname}}</span></div>
                    <div style="font-size:14px"> {{childpro.placeholder}}</div>

                </el-card>
            </el-col>

        </el-row>
        <el-row :gutter="12" v-if="pzoption.datatype=='1'">
            <el-col :xs="12" :sm="12" :md="6" v-for="item in pzoption.dataset" class="mt10">
                <el-card shadow="hover">
                    <div style="font-size: 32px;color: #0498ff;">{{item[pzoption.apicols[1].ColumnName]}}<span style="font-size: 14px;color: #0498ff;"> {{childpro.dwname}}</span></div>
                    <div style="font-size:14px"> {{item[pzoption.apicols[0].ColumnName]}}</div>
                </el-card>
            </el-col>
        </el-row>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="单位">
                    <el-input v-model="childpro.dwname" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="标题">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>

            </el-form>
        </el-dialog>
    </el-col>


</template>

<script>
    module.exports = {
        props: ['pzoption', 'index'],
        methods: {
            delWid: function (wigdetcode) {
                this.$root.nowwidget = {};//没这个删除不掉啊
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro));
            }
        },
        data: function () {
            return {
                dialogInputVisible: false,
                childpro: {
                    placeholder: "占位符",
                    dwname: "单位"

                }
            }
        },
        mounted: function () {
            var chi = this;
            chi.$nextTick(function () {
                if (chi.$root.addchildwig) {
                    chi.$root.addchildwig();//不能缺少
                }
               // this.pzoption.mdwidth = 24;
                if (chi.pzoption.childpro.placeholder) {
                    chi.childpro = chi.pzoption.childpro;
                }
            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            }
        },
        computed: {
            // 仅读取
            chiwid: function () {
                //if (this.pzoption.mdwidth == "6" || this.pzoption.mdwidth == "8") {
                //    return 24;
                //}
                //if (this.pzoption.mdwidth == "12") {
                //    return 12;
                //}
                //if (this.pzoption.mdwidth == "16") {
                //    return 8;
                //}
                //if (this.pzoption.mdwidth == "18") {
                //    return 6;
                //}
                //if (this.pzoption.mdwidth == "24") {
                //    return 6;
                //}
            }
        }
    };
</script>