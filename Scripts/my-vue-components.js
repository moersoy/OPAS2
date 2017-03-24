
// 金额数字输入组件,需要用到外部库currency-validator.js
// 改造自 https://cn.vuejs.org/v2/guide/components.html#使用自定义事件的表单输入组件
Vue.component('currency-input', {
  template: '\
      <input\
        ref="input"\
        v-bind:value="value"\
        v-on:input="updateValue($event.target.value)"\
        v-on:focus="selectAll"\
        v-on:blur="formatValue"\
        class="el-input__inner"\
        style="text-align: right;"\
      >\
  ',
  props: {
    value: {
      type: Number,
      default: 0
    },
  },
  mounted: function () {
    this.formatValue()
  },
  methods: {
    updateValue: function (value) {
      var result = currencyValidator.parse(value, this.value)
      if (result.warning) {
        this.$refs.input.value = result.value
      }
      this.$emit('input', result.value)
    },
    formatValue: function () {
      this.$refs.input.value = currencyValidator.format(this.value)
    },
    selectAll: function (event) {
      // Workaround for Safari bug
      // http://stackoverflow.com/questions/1269722/selecting-text-on-focus-using-jquery-not-working-in-safari-and-chrome
      setTimeout(function () {
        event.target.select()
      }, 0)
    }
  }
})

// 日期显示Filter
Vue.filter('formatDate', function (value) {
  if (value) {
    return moment(String(value)).format('YYYY/MM/DD hh:mm')
  }
});