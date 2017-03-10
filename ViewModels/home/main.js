const v = new Vue({
  el: '#app',
  data: {
    workingMode: workingMode,
    message: 'Home'
  },
  mixins: [opas_vue_public_mixin],
  methods: {
    handleOpen(key, keyPath) {
      console.log(key, keyPath);
    },
    handleClose(key, keyPath) {
      console.log(key, keyPath);
    }
  },
  computed: {

  }
});