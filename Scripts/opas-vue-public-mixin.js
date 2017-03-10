var opas_vue_public_mixin = {
  data: {
  },
  methods: {
    alertErrorNotImplemented() {
      this.$notify.error({
        title: '错误',
        message: '本功能尚未实现'
      });
    },
  }
};