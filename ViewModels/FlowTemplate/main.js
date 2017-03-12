const v = new Vue({
  el: '#app',
  data: {
    message: 'FlowTemplate',
    flowTemplateName: initBag.flowTemplateName,
    flowTemplateDef: initBag.flowTemplateDef,
    currentActivityNodeGuid: initBag.currentActivityNodeGuid,
  },
  mixins: [opas_vue_public_mixin],
  methods: {
  },
  mounted () {
    this.drawFlowChart(this.flowTemplateDef, null);
  }
});