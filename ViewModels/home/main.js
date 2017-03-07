const v = new Vue({
  el: '#app',
  data: {
    workingMode: workingMode,
    message: 'Home'
  },
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