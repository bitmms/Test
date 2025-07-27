<script setup lang="ts">
import {computed} from "vue";
import webSiteJsonData from '~/assets/json/website.json'
import searchJsonData from '~/assets/json/search.json'
import toolOfToUp from '~/assets/svg/tool/tool-to-up.svg'
import toolOfToDown from '~/assets/svg/tool/tool-to-down.svg'
import type Category from "~/assets/ts/Category";
import type SearchVO from "~/assets/ts/SearchVO";
import type SearchEngine from "~/assets/ts/SearchEngine";

// ======= DOM Refs =======
const contextBoxRef: Ref<HTMLElement | null> = ref(null)    // 主区域 dom 元素，便于处理滚动事件

// ======= State =======
const websiteData: Ref<Category[]> = ref(webSiteJsonData)   // json 数据
const searchData: Ref<SearchVO> = ref(searchJsonData)       // json 数据
const searchContent: Ref<string> = ref('')                  // 用户当前等待搜索的字符串
const searchType: Ref<string> = ref('bing')                 // 用户当前使用的搜索引擎名称
const isShowToUp: Ref<boolean> = ref(false)                 // 记录当前是否显示返回底部按钮
const isShowSwitchSearchEngines: Ref<boolean> = ref(false)  // 记录当前是否显示切换搜索引擎的盒子

// ======= 批量预加载指定的 SVG 文件 =======
const svgAssets = import.meta.glob('~/assets/svg/{nav,website,search}/*.svg', {eager: true, import: 'default'})

// 根据传入的路径从预加载的 svgAssets 中获取对应的 SVG 资源路径
const getSvgUrl = (path: string) => {
  if (Object.prototype.hasOwnProperty.call(svgAssets, path)) {
    return svgAssets[path] as string;
  }
  return ''
}

// 返回顶部、返回底部
const returnToUpOrDown = () => {
  if (!contextBoxRef.value) return

  if (isShowToUp.value) {
    isShowToUp.value = false
    contextBoxRef.value.scrollTop = 0
  } else {
    isShowToUp.value = true
    contextBoxRef.value.scrollTop = contextBoxRef.value?.scrollHeight
  }
}

// 监听 main 的滚动事件
const mainContentScroll = () => {
  if (contextBoxRef.value) {
    isShowToUp.value = contextBoxRef.value.scrollTop > 0
  }
}

// 点击左侧导航栏的某项
const selectAsideNavItem = (nowNavItem: Category) => {
  websiteData.value.forEach((item) => (item.isSelected = false))
  nowNavItem.isSelected = true
}

// 鼠标移入左侧导航栏的某项
const mouseEnterAsideNavItem = (nowNavItem: Category) => {
  websiteData.value.forEach((item) => (item.isMouseenter = false))
  if (!nowNavItem.isSelected) {
    nowNavItem.isMouseenter = true
  }
}

// 鼠标移除左侧导航栏的某项
const mouseLeaveAsideNavItem = (nowNavItem: Category) => {
  nowNavItem.isMouseenter = false
}

// 按下回车进行搜素
const enterToSearch = () => {
  const searchString = searchContent.value
  if (searchString.length > 0) {
    const targetUrl = nowSearchEngineObj.value.url + searchString
    if (searchData.value.tab) {
      window.open(targetUrl, '_blank', 'noopener,noreferrer')
    } else {
      window.location.href = targetUrl
    }
    searchContent.value = ''
  }
}

// 是否显示搜索引擎切换选择框
const showSwitchSearchEngines = () => {
  isShowSwitchSearchEngines.value = !isShowSwitchSearchEngines.value
}

// 选择某个搜索引擎
const selectSearchEngine = (name: string) => {
  searchType.value = name
  isShowSwitchSearchEngines.value = false
  window.localStorage.setItem('searchType', name)
}

// 获取当前搜索引擎
const nowSearchEngineObj = computed((): SearchEngine => {
  let defaultSearchEngine: SearchEngine = {
    name: searchType.value,
    url: 'https://cn.bing.com/search?ensearch=0&q=',
    logo: '/assets/svg/search/search-bing.svg',
  }
  for (const searchEngineItem of searchData.value.list) {
    if (searchEngineItem.name === searchType.value) {
      defaultSearchEngine = searchEngineItem
      break
    }
  }
  return defaultSearchEngine
})

// 客户端查询浏览器缓存进行数据的更新
onMounted(() => {
  if (window.localStorage.getItem('searchType') !== null) {
    searchType.value = window.localStorage.getItem('searchType') as string
  }
})

</script>

<template>
  <div id="container">
    <aside class="container-left-box">
      <div class="aside-logo-box">
        <header id="aside-logo-box">
          <a href="/" class="logo" title="双比特 - 程序员导航站" aria-label="返回首页">
            <img
                :src="'/bits-logo.svg'"
                alt="双比特 - 程序员导航站"
                loading="eager"
            >
          </a>
        </header>
      </div>

      <div class="aside-nav-box">
        <div id="aside-nav-box">
          <ul>
            <li v-for="(item, index) in websiteData" :key="index">
              <a :href="`#${item.category}`" :title="item.category" :aria-label="item.category">
                <div
                    :class="`nav-item-box ${item.isSelected ? 'active' : ''} ${item.isMouseenter ? 'mouseenter' : ''}`"
                    @click="selectAsideNavItem(item)"
                    @mouseenter="mouseEnterAsideNavItem(item)"
                    @mouseleave="mouseLeaveAsideNavItem(item)"
                >
                  <div class="nav-icon">
                    <span>
                      <img :src="getSvgUrl(item.iconSvg)" :alt="item.category">
                    </span>
                  </div>
                  <div class="nav-title">
                    <span>{{ item.category }}</span>
                  </div>
                </div>
              </a>
            </li>
          </ul>
        </div>
      </div>
    </aside>

    <main ref="contextBoxRef" class="container-right-box" @scroll.capture="mainContentScroll">
      <div class="header-box">
        <div id="header-box">·
          <div class="search-box">

            <div class="search-select">
              <img
                  :src="getSvgUrl(nowSearchEngineObj.logo)"
                  :alt="nowSearchEngineObj.name"
                  :title="nowSearchEngineObj.name"
                  @click="showSwitchSearchEngines"
              >
            </div>

            <div class="search-input">
              <input v-model="searchContent" type="text" placeholder="输入并搜索..." title="输入并搜索..." @keydown.enter="enterToSearch">
            </div>

            <div v-show="isShowSwitchSearchEngines" class="switch-search">
              <ul>
                <li v-for="(searchEngineItem, index) in searchData.list" :key="index">
                  <img
                      :src="getSvgUrl(searchEngineItem.logo)"
                      :alt="searchEngineItem.name"
                      :title="searchEngineItem.name"
                      @click="selectSearchEngine(searchEngineItem.name)"
                  >
                </li>
              </ul>
            </div>

          </div>
        </div>
      </div>
      <div class="content-box">
        <main id="main-box">
          <section v-for="(categoryItem, categoryIndex) in websiteData" :key="categoryIndex" class="main-section">

            <div :id="categoryItem.category" class="section-title">
              <a :href="`#${categoryItem.category}`" :title="categoryItem.category" :aria-label="categoryItem.category">
                <span class="title-icon"><img :src="getSvgUrl(categoryItem.iconSvg)" :alt="categoryItem.category"></span>
                <span class="title-text">{{ categoryItem.category }}</span>
              </a>
            </div>

            <div class="section-item">
              <ul>
                <li v-for="(websiteItem, websiteIndex) in categoryItem.children" :key="websiteIndex">
                  <a :href="`/redirect/${websiteItem.slug}`" :title="websiteItem.desc" target="_blank">
                    <div class="item-box">
                      <div class="item-left">
                        <div class="web-logo">
                          <span v-if="getSvgUrl(websiteItem.logo) === ''">{{ websiteItem.name.charAt(0) }}</span>
                          <img v-else :src="getSvgUrl(websiteItem.logo)" :alt="websiteItem.name">
                        </div>
                      </div>
                      <div class="item-right">
                        <div class="web-title">{{ websiteItem.name }}</div>
                        <div class="web-desc">{{ websiteItem.desc }}</div>
                      </div>
                    </div>
                  </a>
                </li>
              </ul>
            </div>

          </section>
        </main>
      </div>
      <div class="footer-box">
        <footer id="footer-box">
          <span>Copyright © 2025</span>
          <span>Hello World</span>
          <span>粤ICP备10086号</span>
        </footer>
      </div>
    </main>

    <div class="container-tool-box">
      <div id="tool-box">

        <div class="tool-item-box" @click="returnToUpOrDown">
          <img v-show="isShowToUp" :src="toolOfToUp" alt="返回顶部">
          <img v-show="!isShowToUp" :src="toolOfToDown" alt="滚动到底部">
        </div>

      </div>
    </div>
  </div>
</template>

<style lang="less" scoped>
@media screen and (min-width: 1201px) {
  /* 变量 */
  #container {
    --aside-width: 220px;
    --content-grid: 1fr 1fr 1fr 1fr;
    --header-display: block;

    --transition-time: .2s;
  }

  /* 结构 */
  #container {
    position: relative;
    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: row;
    background: #f5f5f5;

    .container-left-box {
      position: relative;
      width: var(--aside-width);
      height: 100%;
      display: flex;
      flex-direction: column;
      overflow: hidden;

      .aside-logo-box {
        width: 100%;
        height: 80px;
      }

      .aside-nav-box {
        flex: 1;
        overflow-y: scroll;

        &::-webkit-scrollbar {
          width: 0;
        }
      }
    }

    .container-left-box::after {
      content: '';
      position: absolute;
      top: 0;
      right: 0;
      width: 4px;
      height: 100%;
      background: #ffffff;
    }

    .container-right-box {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow-y: scroll;

      .header-box {
        width: 100%;
        height: auto;
        padding-left: 0;
        padding-right: 0;
        background: rgba(255, 255, 255, 0.6);
      }

      .content-box {
        width: 100%;
        height: auto;
        padding: 3%;
      }

      .footer-box {
        width: 100%;
        height: auto;
        padding-left: 0;
        padding-right: 0;
        background: rgba(255, 255, 255, 0.6);
      }
    }

    .container-tool-box {
      position: absolute;
      right: 25px;
      bottom: 25px;
      width: 40px;
      height: auto;
    }
  }

  /* 具体的样式 */
  #container {
    #aside-logo-box {
      width: 100%;
      height: 100%;

      .logo {
        position: relative;
        display: block;
        width: 100%;
        height: 100%;

        img {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          display: block;
          width: 80%;
          cursor: pointer;
        }
      }
    }

    #aside-nav-box {
      width: 100%;

      ul {
        width: 100%;
        height: 100%;

        li {
          width: 100%;
          height: 40px;
          padding: 0 15px;

          .nav-item-box {
            display: flex;
            flex-direction: row;
            width: 100%;
            height: 40px;
            border-radius: 8px;
            cursor: pointer;
            transition: all var(--transition-time);

            .nav-icon {
              width: 40px;
              height: 40px;

              span {
                position: relative;
                display: block;
                width: 100%;
                height: 100%;

                img {
                  position: absolute;
                  top: 50%;
                  left: 60%;
                  transform: translateY(-50%);
                  width: 50%;
                  height: 50%;
                }
              }
            }

            .nav-title {
              flex: 1;

              span {
                display: block;
                width: 100%;
                height: 100%;
                text-align: left;
                line-height: 40px;
                font-size: 14px;
                color: #333333;
                padding-left: 10px;
              }
            }
          }

          .mouseenter {
            background: #ffffff;
          }

          .active {
            background: #e0e0e0;
          }
        }

        li:nth-child(n + 2) {
          margin-top: 12px;
        }
      }
    }

    #header-box {
      position: relative;
      width: 100%;
      height: 200px;
      background: url("~/assets/img/ad/01.png") no-repeat center center fixed;
      display: var(--header-display);

      .search-box {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        width: 800px;
        height: 80px;
        border-radius: 5px;
        z-index: 999;
        display: flex;
        flex-direction: row;
        background: #cfd3db;

        .search-select {
          position: relative;
          width: 80px;
          height: 80px;

          img {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            display: block;
            width: 55%;
            height: 55%;
            cursor: pointer;
          }
        }

        .search-input {
          flex: 1;

          input {
            display: block;
            width: 100%;
            height: 100%;
            border: none;
            outline: none;
            color: #222226;
            font-size: 20px;
            font-weight: 400;
            background: #d0d6de;
          }
        }

        .switch-search {
          position: absolute;
          top: 85px;
          left: 0;
          border-radius: 5px;
          width: 100%;
          height: auto;
          background: #cfd3db;

          ul {
            width: 100%;
            height: auto;

            li {
              position: relative;
              float: left;
              display: block;
              width: 80px;
              height: 80px;
              text-align: center;
              line-height: 80px;

              img {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                display: block;
                width: 60%;
                height: 60%;
                transition: all var(--transition-time);
                cursor: pointer;

                &:hover {
                  scale: 1.05;
                }
              }
            }
          }
        }
      }
    }

    #main-box {
      width: 100%;
      height: auto;

      .main-section {
        width: 100%;
        height: auto;
        margin-top: 40px;

        .section-title {
          width: 100%;
          height: 30px;
          margin-bottom: 10px;

          a {
            display: inline-block;
            transition: all var(--transition-time);

            .title-icon {
              position: relative;
              float: left;
              width: 30px;
              height: 30px;

              img {
                position: absolute;
                top: 50%;
                left: 0;
                transform: translateY(-50%);
                width: 80%;
                height: 80%;
              }
            }

            .title-text {
              font-weight: 600;
              color: #333333;
              line-height: 30px;
              font-size: 16px;
            }
          }

          a:hover {
            scale: 1.05;
          }
        }

        .section-item {
          display: block;
          width: 100%;
          height: auto;

          ul {
            width: 100%;
            height: auto;
            display: grid;
            grid-template-columns: var(--content-grid);
            grid-row-gap: 25px;
            grid-column-gap: 25px;

            li {
              height: 84px;
              border-radius: 4px;
              box-shadow: 0 2px 8px rgb(60 114 139 / 3%);
              background: rgba(255, 255, 255, 0.6);
              transition: all var(--transition-time);
              cursor: pointer;
              overflow: hidden;

              a {
                display: block;
                width: 100%;
                height: 100%;
                padding: 12px;

                .item-box {
                  display: flex;
                  width: 100%;
                  height: 60px;

                  .item-left {
                    width: 60px;
                    height: 60px;
                    padding: 10px;

                    .web-logo {
                      position: relative;
                      width: 40px;
                      height: 40px;

                      span {
                        display: block;
                        width: 40px;
                        height: 40px;
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        text-align: center;
                        line-height: 40px;
                        font-size: 20px;
                        font-weight: 500;
                        background: #007dfe;
                        color: #ffffff;
                        border-radius: 50%;
                      }

                      img {
                        display: block;
                        width: 40px;
                        height: 40px;
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        border-radius: 20%;
                      }
                    }
                  }

                  .item-right {
                    flex: 1;
                    display: flex;
                    flex-direction: column;
                    padding: 10px 0;
                    overflow: hidden;

                    .web-title {
                      flex: 1;
                      display: block;
                      font-size: 14px;
                      font-weight: 500;
                      color: #333333;
                      line-height: 20px;
                      margin-bottom: 2px;
                      white-space: nowrap;
                      text-overflow: ellipsis;
                      overflow: hidden;
                    }

                    .web-desc {
                      flex: 1;
                      display: block;
                      color: #999999;
                      font-size: 12px;
                      line-height: 20px;
                      white-space: nowrap;
                      text-overflow: ellipsis;
                      overflow: hidden;
                    }
                  }
                }
              }
            }

            li:hover {
              transform: translateY(-5%) scale(1.02);
            }
          }
        }
      }

      .main-section:nth-child(1) {
        margin-top: 0;
      }
    }

    #footer-box {
      width: 100%;
      height: 60px;
      text-align: center;
      line-height: 60px;
      margin-top: 60px;

      span {
        color: #666666;
        font-size: 0.8em;
      }
    }

    #tool-box {
      width: 40px;
      height: auto;

      .tool-item-box {
        position: relative;
        width: 40px;
        height: 40px;
        background: #ffffff;
        border-radius: 50%;
        cursor: pointer;
        transition: all var(--transition-time);

        img {
          display: block;
          width: 65%;
          height: 65%;
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
        }
      }

      .tool-item-box:hover {
        scale: 1.1;
      }

      .tool-item-box:nth-child(n + 2) {
        margin-top: 10px;
      }
    }
  }
}

@media screen and (max-width: 1200px) {
  /* 变量 */
  #container {
    --aside-width: 0;
    --content-grid: 1fr 1fr;
    --header-display: none;

    --transition-time: .2s;
  }

  /* 结构 */
  #container {
    position: relative;
    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: row;
    background: #f5f5f5;

    .container-left-box {
      position: relative;
      width: var(--aside-width);
      height: 100%;
      display: flex;
      flex-direction: column;
      overflow: hidden;

      .aside-logo-box {
        width: 100%;
        height: 80px;
      }

      .aside-nav-box {
        flex: 1;
        overflow-y: scroll;

        &::-webkit-scrollbar {
          width: 0;
        }
      }
    }

    .container-left-box::after {
      content: '';
      position: absolute;
      top: 0;
      right: 0;
      width: 4px;
      height: 100%;
      background: #ffffff;
    }

    .container-right-box {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow-y: scroll;

      .header-box {
        width: 100%;
        height: auto;
        padding-left: 0;
        padding-right: 0;
        background: rgba(255, 255, 255, 0.6);
      }

      .content-box {
        width: 100%;
        height: auto;
        padding: 3%;
      }

      .footer-box {
        width: 100%;
        height: auto;
        padding-left: 0;
        padding-right: 0;
        background: rgba(255, 255, 255, 0.6);
      }
    }

    .container-tool-box {
      position: absolute;
      right: 25px;
      bottom: 25px;
      width: 40px;
      height: auto;
    }
  }

  /* 具体的样式 */
  #container {
    #aside-logo-box {
      width: 100%;
      height: 100%;

      .logo {
        position: relative;
        display: block;
        width: 100%;
        height: 100%;

        img {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          display: block;
          width: 80%;
          cursor: pointer;
        }
      }
    }

    #aside-nav-box {
      width: 100%;

      ul {
        width: 100%;
        height: 100%;

        li {
          width: 100%;
          height: 40px;
          padding: 0 15px;

          .nav-item-box {
            display: flex;
            flex-direction: row;
            width: 100%;
            height: 40px;
            border-radius: 8px;
            cursor: pointer;
            transition: all var(--transition-time);

            .nav-icon {
              width: 40px;
              height: 40px;

              span {
                position: relative;
                display: block;
                width: 100%;
                height: 100%;

                img {
                  position: absolute;
                  top: 50%;
                  left: 60%;
                  transform: translateY(-50%);
                  width: 50%;
                  height: 50%;
                }
              }
            }

            .nav-title {
              flex: 1;

              span {
                display: block;
                width: 100%;
                height: 100%;
                text-align: left;
                line-height: 40px;
                font-size: 14px;
                color: #333333;
                padding-left: 10px;
              }
            }
          }

          .mouseenter {
            background: #ffffff;
          }

          .active {
            background: #e0e0e0;
          }
        }

        li:nth-child(n + 2) {
          margin-top: 12px;
        }
      }
    }

    #header-box {
      position: relative;
      width: 100%;
      height: 200px;
      background: url("~/assets/img/ad/01.png") no-repeat center center fixed;
      display: var(--header-display);

      .search-box {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        width: 800px;
        height: 80px;
        border-radius: 5px;
        z-index: 999;
        display: flex;
        flex-direction: row;
        background: #cfd3db;

        .search-select {
          position: relative;
          width: 80px;
          height: 80px;

          img {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            display: block;
            width: 55%;
            height: 55%;
            cursor: pointer;
          }
        }

        .search-input {
          flex: 1;

          input {
            display: block;
            width: 100%;
            height: 100%;
            border: none;
            outline: none;
            color: #222226;
            font-size: 20px;
            font-weight: 400;
            background: #d0d6de;
          }
        }

        .switch-search {
          position: absolute;
          top: 85px;
          left: 0;
          border-radius: 5px;
          width: 100%;
          height: auto;
          background: #cfd3db;

          ul {
            width: 100%;
            height: auto;

            li {
              position: relative;
              float: left;
              display: block;
              width: 80px;
              height: 80px;
              text-align: center;
              line-height: 80px;

              img {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                display: block;
                width: 60%;
                height: 60%;
                transition: all var(--transition-time);
                cursor: pointer;

                &:hover {
                  scale: 1.05;
                }
              }
            }
          }
        }
      }
    }

    #main-box {
      width: 100%;
      height: auto;

      .main-section {
        width: 100%;
        height: auto;
        margin-top: 40px;

        .section-title {
          width: 100%;
          height: 30px;
          margin-bottom: 10px;

          a {
            display: inline-block;
            transition: all var(--transition-time);

            .title-icon {
              position: relative;
              float: left;
              width: 30px;
              height: 30px;

              img {
                position: absolute;
                top: 50%;
                left: 0;
                transform: translateY(-50%);
                width: 80%;
                height: 80%;
              }
            }

            .title-text {
              font-weight: 600;
              color: #333333;
              line-height: 30px;
              font-size: 16px;
            }
          }

          a:hover {
            scale: 1.05;
          }
        }

        .section-item {
          display: block;
          width: 100%;
          height: auto;

          ul {
            width: 100%;
            height: auto;
            display: grid;
            grid-template-columns: var(--content-grid);
            grid-row-gap: 25px;
            grid-column-gap: 25px;

            li {
              height: 84px;
              border-radius: 4px;
              box-shadow: 0 2px 8px rgb(60 114 139 / 3%);
              background: rgba(255, 255, 255, 0.6);
              transition: all var(--transition-time);
              cursor: pointer;
              overflow: hidden;

              a {
                display: block;
                width: 100%;
                height: 100%;
                padding: 12px;

                .item-box {
                  display: flex;
                  width: 100%;
                  height: 60px;

                  .item-left {
                    width: 60px;
                    height: 60px;
                    padding: 10px;

                    .web-logo {
                      position: relative;
                      width: 40px;
                      height: 40px;

                      span {
                        display: block;
                        width: 40px;
                        height: 40px;
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        text-align: center;
                        line-height: 40px;
                        font-size: 20px;
                        font-weight: 500;
                        background: #007dfe;
                        color: #ffffff;
                        border-radius: 50%;
                      }

                      img {
                        display: block;
                        width: 40px;
                        height: 40px;
                        position: absolute;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        border-radius: 20%;
                      }
                    }
                  }

                  .item-right {
                    flex: 1;
                    display: flex;
                    flex-direction: column;
                    padding: 10px 0;
                    overflow: hidden;

                    .web-title {
                      flex: 1;
                      display: block;
                      font-size: 14px;
                      font-weight: 500;
                      color: #333333;
                      line-height: 20px;
                      margin-bottom: 2px;
                      white-space: nowrap;
                      text-overflow: ellipsis;
                      overflow: hidden;
                    }

                    .web-desc {
                      flex: 1;
                      display: block;
                      color: #999999;
                      font-size: 12px;
                      line-height: 20px;
                      white-space: nowrap;
                      text-overflow: ellipsis;
                      overflow: hidden;
                    }
                  }
                }
              }
            }

            li:hover {
              transform: translateY(-5%) scale(1.02);
            }
          }
        }
      }

      .main-section:nth-child(1) {
        margin-top: 0;
      }
    }

    #footer-box {
      width: 100%;
      height: 60px;
      text-align: center;
      line-height: 60px;
      margin-top: 60px;

      span {
        color: #666666;
        font-size: 0.8em;
      }
    }

    #tool-box {
      width: 40px;
      height: auto;

      .tool-item-box {
        position: relative;
        width: 40px;
        height: 40px;
        background: #ffffff;
        border-radius: 50%;
        cursor: pointer;
        transition: all var(--transition-time);

        img {
          display: block;
          width: 65%;
          height: 65%;
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
        }
      }

      .tool-item-box:hover {
        scale: 1.1;
      }

      .tool-item-box:nth-child(n + 2) {
        margin-top: 10px;
      }
    }
  }
}
</style>