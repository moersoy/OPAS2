"use strict";!function e(r,n,o){function t(i,f){if(!n[i]){if(!r[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var l=n[i]={exports:{}};r[i][0].call(l.exports,function(e){var n=r[i][1][e];return t(n?n:e)},l,l.exports,e,r,n,o)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<o.length;i++)t(o[i]);return t}({1:[function(e,r,n){new Vue({el:"#app",data:{workingMode:workingMode,message:"Home"},methods:{handleOpen:function(e,r){console.log(e,r)},handleClose:function(e,r){console.log(e,r)}},computed:{}})},{}]},{},[1]);