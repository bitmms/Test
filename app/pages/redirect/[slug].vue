<script setup lang="ts">
import {computed} from 'vue';
import {useRoute} from 'vue-router';
import webSiteJsonData from '~/assets/json/website.json'
import type WebSite from "~/assets/ts/WebSite";
import type Category from "~/assets/ts/Category";

// ======= 路由参数 =======
const route = useRoute();
const slug = route.params.slug as string;

// ======= State =======
const websiteData: Ref<Category[]> = ref(webSiteJsonData)   // json 数据

// ======= 批量预加载指定的 SVG 文件 =======
const svgAssets = import.meta.glob('~/assets/svg/{default,nav,website}/*.svg', {eager: true, import: 'default'})

// 根据传入的路径从预加载的 svgAssets 中获取对应的 SVG 资源路径
const getSvgUrl = (path: string) => {
  if (Object.prototype.hasOwnProperty.call(svgAssets, path)) {
    return svgAssets[path] as string;
  }
  return ''
}

// 从 dataJson 中找到符合要求的 WebSite 对象
const targetWebSiteInfo = computed(() => {
  const tempWebSIteInfo: WebSite = {
    name: '目标不存在',
    desc: '点击返回主页',
    logo: `/assets/svg/default/defaultNotFound-404-${new Date().getSeconds() % 10}.svg`,
    href: '/',
    slug: '/',
  }
  for (const categoryItem of websiteData.value) {
    for (const websiteItem of categoryItem.children) {
      if (websiteItem.slug === slug) {
        return websiteItem;
      }
    }
  }
  return tempWebSIteInfo;
})

</script>

<template>
  <div id="redirect-container">

    <div class="logo-box">
      <span v-if="getSvgUrl(targetWebSiteInfo.logo) === ''">{{ targetWebSiteInfo.name.charAt(0) }}</span>
      <img v-else :src="getSvgUrl(targetWebSiteInfo.logo)" :alt="targetWebSiteInfo.name">
    </div>

    <div class="title-box">
      <h1 class="title">{{ targetWebSiteInfo.name }}</h1>
    </div>

    <div class="desc-box">
      <p class="description">{{ targetWebSiteInfo.desc }}</p>
    </div>

    <div class="button-box">
      <a :href="targetWebSiteInfo.href" class="btn1">立即前往</a>
      <a href="/" class="btn2">返回首页</a>
    </div>

  </div>
</template>

<style lang="less" scoped>
#redirect-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 90%;
  max-width: 500px;
  padding: 30px;
  background: #ffffff;
  border-radius: 1rem;
  box-shadow: rgba(0, 0, 0, 0.2) 0 10px 30px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;

  .logo-box {
    height: 80px;
    display: flex;
    justify-content: center;
    align-items: center;

    img {
      height: 80px;
      object-fit: contain;
    }

    span {
      display: block;
      width: 80px;
      height: 80px;
      line-height: 80px;
      text-align: center;
      font-size: 36px;
      border-radius: 50%;
      font-weight: 500;
      background: #007dfe;
      color: #ffffff;
    }
  }

  .title-box h1 {
    font-size: 32px;
    font-weight: bold;
    text-align: center;
  }

  .desc-box p {
    color: #6b7280;
    text-align: center;
    line-height: 1.5;
  }

  .button-box {
    display: flex;
    justify-content: space-around;
    width: 100%;

    .btn1 {
      background: #3b82f6;
      color: white;

      padding: 10px 20px;
      font-size: 1rem;
      font-weight: 500;
      border-radius: 0.5rem;
      text-align: center;
      transition: background 0.3s ease;

      &:hover {
        background: #60a5fa;
      }
    }

    .btn2 {
      background: #f3f4f6;
      color: #374151;

      padding: 10px 20px;
      font-size: 1rem;
      font-weight: 500;
      border-radius: 0.5rem;
      text-align: center;
      transition: background 0.3s ease;

      &:hover {
        background: #e5e7eb;
      }
    }
  }
}
</style>
