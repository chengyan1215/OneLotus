<template>
    <el-col :sm="24" :md="24">
        <el-table :data="pzoption.dataset" style="width: 100%" stripe border fit @row-click="rowclick">
            <el-table-column :label="pzoption.title">
                <el-table-column type="index" width="40" fixed> </el-table-column>

                <el-table-column label="日期"
                                 width="180">
                    <template slot-scope="scope">
                        <el-input v-model="scope.row.date" placeholder="请输入内容"></el-input>
                    </template>
                </el-table-column>
                <el-table-column label="姓名"
                                 width="180">
                    <template slot-scope="scope">
                        <el-input v-model="scope.row.name" placeholder="请输入内容"></el-input>
                    </template>
                </el-table-column>
                <el-table-column label="地址">
                    <template slot-scope="scope">
                        <el-input v-model="scope.row.address" placeholder="请输入内容"></el-input>
                    </template>
                </el-table-column>

                <el-table-column fixed="right" label="操作" width="50">
                    <template slot-scope="scope">
                        <el-button @click.native.prevent="delRow(scope.$index,pzoption.dataset)"
                                   type="text"
                                   size="small">
                            删除
                        </el-button>

                    </template>
                </el-table-column>
            </el-table-column>
        </el-table>
        <el-button type="primary" size="mini" class="mt10 pull-right" @click="addRow()">添加行<i class="el-icon-plus"></i></el-button>
    </el-col>
</template>

<script>
    module.exports = {
        data() {
            return {
                pzoption: { dataset: [], title: "计划明细" }
            };
        },
        props: [],
        methods: {
            addRow: function () {
                this.pzoption.dataset.push({ date: "", name: "", address: "" });
            },
            delRow: function (index, rows) {
                rows.splice(index, 1);
            },
            rowclick: function (row, event, column) {

            },
            GetData: function () {
                return this.pzoption;
            },
            SelDraft: function (obj) {
                this.pzoption = obj;
            }

        },
        created() {
            this.$nextTick(function () {
                // DOM 更新了
                // console.debug(this.$refs.refCol)
            })
        }
    }
</script>

