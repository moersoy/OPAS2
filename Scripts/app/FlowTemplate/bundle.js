"use strict";

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

(function e(t, n, r) {
  function s(o, u) {
    if (!n[o]) {
      if (!t[o]) {
        var a = typeof require == "function" && require;if (!u && a) return a(o, !0);if (i) return i(o, !0);var f = new Error("Cannot find module '" + o + "'");throw f.code = "MODULE_NOT_FOUND", f;
      }var l = n[o] = { exports: {} };t[o][0].call(l.exports, function (e) {
        var n = t[o][1][e];return s(n ? n : e);
      }, l, l.exports, e, t, n, r);
    }return n[o].exports;
  }var i = typeof require == "function" && require;for (var o = 0; o < r.length; o++) {
    s(r[o]);
  }return s;
})({ 1: [function (require, module, exports) {
    var ActivityConnectionDraw = function () {
      function ActivityConnectionDraw() {
        _classCallCheck(this, ActivityConnectionDraw);
      }

      _createClass(ActivityConnectionDraw, [{
        key: "drawConnection",
        // 节点间连线绘图对象
        // 绘制(并创建)节点间连线绘图对象
        value: function drawConnection(obj, drawingPaper) {
          // when draw the first time, need validate it 为简单起见两个节点间的同一方向连线不能超过两条
          if (!obj.element && _.filter(drawingPaper.connections, function (c) {
            return c.obj1 === obj.obj1 && c.obj2 === obj.obj2;
          }).length >= 2) {
            console.error(3, '两个节点间的同一方向连线不能超过两条!', 3);
            return null;
          }

          var _connectionsBefore = _.take(drawingPaper.connections, obj.element ? _.findIndex(drawingPaper.connections, { guid: obj.guid }) : drawingPaper.connections.length);
          var _conIndex = _.filter(_connectionsBefore, function (c) {
            return (// c.guid != obj.guid && 
              c.obj1 === obj.obj1 && c.obj2 === obj.obj2 || c.obj1 === obj.obj2 && c.obj2 === obj.obj1
            );
          }).length;
          var pointsPair = this.getStartEnd(obj.obj1, obj.obj2, _conIndex);
          var _path = this.getConnection(pointsPair.start.x, pointsPair.start.y, pointsPair.end.x, pointsPair.end.y);
          if (obj.element) {
            // redraw
            obj.element.attr({ path: _path });
          } else {
            // draw the first time, need initialize it
            obj.guid = obj.guid || genGuid();
            obj.name = obj.name || '未命名连接' + Date.now().toString();
            obj.element = drawingPaper.paper.path(_path);
            obj.element.hover(function (e) {
              e.currentTarget.style.cursor = 'hand';
            }, function (e) {
              e.currentTarget.style.cursor = 'pointer';
            });
            drawingPaper.connections.push(obj);
          }
          obj.element.attr({ 'stroke-dasharray': '', stroke: 'blue', 'stroke-width': 2 });
          obj.element.data('guid', obj.guid);
          obj.element.toBack();
          return obj;
        }
      }, {
        key: "getStartEnd",


        // 获取连接起始与结束对象的连线的起始Point与结束Point
        value: function getStartEnd(obj1, obj2, index) {
          var bb1 = obj1.getBBox(),
              bb2 = obj2.getBBox();
          var p = [{ x: bb1.x + bb1.width / 2, y: bb1.y - 1 }, // top
          { x: bb1.x + bb1.width / 2, y: bb1.y + bb1.height + 1 }, // bottom
          { x: bb1.x - 1, y: bb1.y + bb1.height / 2 }, // left
          { x: bb1.x + bb1.width + 1, y: bb1.y + bb1.height / 2 }, // right
          { x: bb1.x - 1, y: bb1.y - 1 }, // top-left
          { x: bb1.x + bb1.width + 1, y: bb1.y - 1 }, // top-right
          { x: bb1.x - 1, y: bb1.y + bb1.height + 1 }, // bottom-left
          { x: bb1.x + bb1.width + 1, y: bb1.y + bb1.height + 1 }, // bottom-right

          { x: bb2.x + bb2.width / 2, y: bb2.y - 1 }, // top
          { x: bb2.x + bb2.width / 2, y: bb2.y + bb2.height + 1 }, // bottom
          { x: bb2.x - 1, y: bb2.y + bb2.height / 2 }, // left
          { x: bb2.x + bb2.width + 1, y: bb2.y + bb2.height / 2 }, // right
          { x: bb2.x - 1, y: bb2.y - 1 }, // top-left
          { x: bb2.x + bb2.width + 1, y: bb2.y - 1 }, // top-right
          { x: bb2.x - 1, y: bb2.y + bb2.height + 1 }, // bottom-left
          { x: bb2.x + bb2.width + 1, y: bb2.y + bb2.height + 1 }];

          var disArray = [];
          for (var i = 0; i < 8; i++) {
            for (var j = 8; j < 16; j++) {
              disArray.push({
                distance: Math.abs(p[i].x - p[j].x) + Math.abs(p[i].y - p[j].y),
                start: p[i], end: p[j]
              });
            }
          }
          var result = _.sortBy(disArray, 'distance')[index || 0];
          return result;
        }

        // 获取组成箭头的三条线段的路径数组

      }, {
        key: "getConnection",
        value: function getConnection(x1, y1, x2, y2, size) {
          size = size || 10;
          var closeAngle = 20;
          var angle = Raphael.angle(x1, y1, x2, y2); // 得到两点之间的角度
          var aClose1 = Raphael.rad(angle - closeAngle); // 角度转换成弧度
          var aClose2 = Raphael.rad(angle + closeAngle);
          var x2a = x2 + Math.cos(aClose1) * size;
          var y2a = y2 + Math.sin(aClose1) * size;
          var x2b = x2 + Math.cos(aClose2) * size;
          var y2b = y2 + Math.sin(aClose2) * size;
          var result = ['M', x1, y1, 'L', x2, y2, 'L', x2a, y2a, 'M', x2, y2, 'L', x2b, y2b];
          return result;
        }
      }]);

      return ActivityConnectionDraw;
    }();

    var ActivityNodeDraw = function () {
      _createClass(ActivityNodeDraw, [{
        key: "getNodeNameShortVersion",
        // 节点绘图对象
        value: function getNodeNameShortVersion(name) {
          return name.length > 6 ? name.substr(0, 4) + '...' : name;
        }
        // 绘制节点

      }, {
        key: "drawFromNodeData",
        value: function drawFromNodeData(nodeData, drawingPaper) {
          var result = void 0,
              ele = void 0,
              txtElement = void 0;
          var x = nodeData.position[0],
              y = nodeData.position[1];
          var txtX = x + 32,
              txtY = y + 20;
          var imgHeight = 40,
              imgWidth = 40;
          var rectHeight = 40,
              rectWidth = 80,
              rectRadius = 5;
          var nodeNameDisplayed = this.getNodeNameShortVersion(nodeData.name);
          switch (nodeData.type) {
            case 'st-start':
              ele = drawingPaper.paper.image('/images/flow_start.png', x, y, imgWidth, imgHeight);
              break;
            case 'st-end':
              ele = drawingPaper.paper.image('/images/flow_end.png', x, y, imgWidth, imgHeight);
              break;
            case 'st-singleHumanActivity':
              ele = drawingPaper.paper.rect(x, y, rectWidth, rectHeight, rectRadius);
              ele.attr({ fill: '#43C8F7', title: nodeData.name });
              txtElement = drawingPaper.paper.text(txtX, txtY, nodeNameDisplayed);
              break;
            case 'st-multiHumanActivity':
              ele = drawingPaper.paper.rect(x, y, rectWidth, rectHeight, rectRadius);
              ele.attr({ fill: '#F1C8F7', title: nodeData.name });
              txtElement = drawingPaper.paper.text(txtX, txtY, nodeNameDisplayed);
              break;
            case 'st-autoActivity':
              ele = drawingPaper.paper.rect(x, y, rectWidth, rectHeight, rectRadius);
              ele.attr({ fill: '#F1F10D', title: nodeData.name });
              txtElement = drawingPaper.paper.text(txtX, txtY, nodeNameDisplayed);
              break;
            default:
              console.info("No drawing handler for type'" + nodeData.type + '"');
              return null;
          }

          ele.data('guid', nodeData.guid);
          result = new ActivityNodeDraw(nodeData.guid, ele, drawingPaper);
          if (txtElement) {
            txtElement.data('guid', nodeData.guid);
            result.textElement = txtElement;
          }
          drawingPaper.nodeDrawElements.push(result);
          // 以下为根据节点的位置自动扩张流程图绘图区域
          if (drawingPaper.paper.width - 50 < x + 80) {
            drawingPaper.paper.setSize(drawingPaper.paper.width + 200, drawingPaper.paper.height);
          }
          if (drawingPaper.paper.height - 50 < y + 40) {
            drawingPaper.paper.setSize(drawingPaper.paper.width, drawingPaper.paper.height + 200);
          }
          return result;
        }
      }]);

      function ActivityNodeDraw(guid, element, drawingPaper) {
        _classCallCheck(this, ActivityNodeDraw);

        this.guid = guid;
        this.element = element;
        this.drawingPaper = drawingPaper;
      }

      return ActivityNodeDraw;
    }();

    var DrawingPaper = function () {
      function DrawingPaper(paper) {
        _classCallCheck(this, DrawingPaper);

        this.paper = paper;
        this.drawActivityType = '_';
        this.drawingTempPath = null; // 正在画的临时连接Path
        this.selectedConnectionGuid = ''; // 正在被选中的连接GUID
        this.selectedNodeGuid = ''; // 正在被选中的节点GUID

        this.connectionsData = []; // 节点间连线数据对象数组
        this.connections = []; // 节点间连线绘图对象数组
        this.nodesData = []; // 节点数据对象数组
        this.nodeDrawElements = []; // 节点绘图对象数组

        this.workflowTemplateName = '';
      }

      // 根据节点数据对象数组和节点间连线数据对象数组绘制流程图


      _createClass(DrawingPaper, [{
        key: "render",
        value: function render() {
          var _this = this;

          // 绘制节点
          var nodeDraw = new ActivityNodeDraw();
          _.each(this.nodesData, function (n) {
            nodeDraw.drawFromNodeData(n, _this);
          });

          // 绘制连接
          var connectionDraw = new ActivityConnectionDraw();
          _.each(this.connectionsData, function (con) {
            if (_this.findNodeDrawByGuid(con.fromGuid) && _this.findNodeDrawByGuid(con.toGuid)) {
              connectionDraw.drawConnection({
                obj1: _this.findNodeDrawByGuid(con.fromGuid).element,
                obj2: _this.findNodeDrawByGuid(con.toGuid).element,
                guid: con.guid
              }, _this);
            }
          });
        }
      }, {
        key: "findNodeDrawByGuid",
        value: function findNodeDrawByGuid(guid) {
          return _.find(this.nodeDrawElements, function (n) {
            return n.guid == guid;
          });
        }
      }, {
        key: "findNodeDataByGuid",
        value: function findNodeDataByGuid(guid) {
          return _.find(this.nodesData, function (n) {
            return n.guid == guid;
          });
        }
      }]);

      return DrawingPaper;
    }();

    var v = new Vue({
      el: '#app',
      data: {
        message: 'FlowTemplate',
        flowTemplateName: initBag.flowTemplateName,
        flowTemplateDef: initBag.flowTemplateDef
      },
      mixins: [opas_vue_public_mixin],
      methods: {},
      mounted: function mounted() {
        if (this.flowTemplateDef) {
          //console.log(this.flowTemplateDef);
          var raphael = Raphael('FlowChart', $('FlowChart').width(), $('FlowChart').height());
          var drawingPaper = new DrawingPaper(raphael);
          drawingPaper.nodesData = this.flowTemplateDef.activityNodes.nodes;
          drawingPaper.connectionsData = this.flowTemplateDef.activityConnections.connections;
          drawingPaper.render();
        }
      }
    });
  }, {}] }, {}, [1]);