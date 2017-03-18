const v = new Vue({
  el: '#app',
  data: {
    backendError: window.initBag && window.initBag.backendError,
  },
  mixins: [opas_vue_public_mixin],
  methods: {
  }
});