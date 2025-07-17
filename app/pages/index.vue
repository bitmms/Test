<script setup lang="ts">
import type {Category} from "~/assets/ts/Category";

import ToUpIcon from '@/components/icons/ToUpIcon.vue'
import ToDownIcon from '@/components/icons/ToDownIcon.vue'
import data from '@/assets/json/data.json'
import defaultNavIcon from '~/assets/svg/nav/defaultNavIcon.svg'

// === DOM Refs ===
const contextBoxRef: Ref<HTMLElement | null> = ref(null)

// === State ===
const isShowToUp: Ref<boolean> = ref(false)
const htmlData: Ref<Category[]> = ref([])

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
  htmlData.value.forEach((item) => (item.isSelected = false))
  nowNavItem.isSelected = true
}

// 鼠标移入左侧导航栏的某项
const mouseEnterAsideNavItem = (nowNavItem: Category) => {
  htmlData.value.forEach((item) => (item.isMouseenter = false))
  if (!nowNavItem.isSelected) {
    nowNavItem.isMouseenter = true
  }
}

// 鼠标移除左侧导航栏的某项
const mouseLeaveAsideNavItem = (nowNavItem: Category) => {
  nowNavItem.isMouseenter = false
}

// 初始化 htmlData 数据
const initHtmlData = async () => {
  // 1. 加载全部的 json 数据
  htmlData.value = data;
  // 2. 加载所有的 svg 文件
  const svgs: Record<string, { default: string }> = import.meta.glob('/assets/svg/nav/*.svg', {eager: true});
  // 3. 封装 svg 文件
  htmlData.value.forEach((item) => {
    const itemSvg = svgs[item.iconSvg];
    item.iconSvg = (itemSvg !== undefined && itemSvg !== null) ? itemSvg.default : defaultNavIcon;
  })
}

// 声明周期
onMounted(() => {
  initHtmlData()
})
</script>

<template>
  <div id="container">
    <aside class="container-left-box">
      <div class="aside-logo-box">
        <div id="aside-logo-box">
          <div class="logo">
            <img src="../assets/img/todo/logo.png" alt="logoALT SEO">
            <div class="logo-desc">拖动 Logo 到书签栏，立即收藏本站</div>
          </div>
        </div>
      </div>

      <div class="aside-nav-box">
        <div id="aside-nav-box">
          <ul>
            <li v-for="(item, index) in htmlData" :key="index">
              <a :href="'#' + item.category">
                <div
                    class="nav-item-box"
                    :class="
                    (item.isSelected ? 'active' : '') +
                    ' ' +
                    (item.isMouseenter ? 'mouseenter' : '')
                  "
                    @click="selectAsideNavItem(item)"
                    @mouseenter="mouseEnterAsideNavItem(item)"
                    @mouseleave="mouseLeaveAsideNavItem(item)"
                >
                  <div class="nav-icon">
                    <span>
                      <img :src="item.iconSvg" :alt="item.category">
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

      <div class="aside-bottom-box">
        <div id="aside-bottom-box"/>
      </div>
    </aside>

    <main ref="contextBoxRef" class="container-right-box" @scroll.capture="mainContentScroll">
      <div class="content-box">
        <main id="main-box">
          <section
              v-for="(categoryItem, categoryIndex) in htmlData"
              :key="categoryIndex"
              class="main-section"
          >
            <div :id="categoryItem.category" class="section-title">
              <span class="title-icon"><img :src="categoryItem.iconSvg" :alt="categoryItem.category"></span>
              <span class="title-text">{{ categoryItem.category }}</span>
            </div>
            <div class="section-item">
              <ul>
                <li
                    v-for="(websiteItem, websiteIndex) in categoryItem.children"
                    :key="websiteIndex"
                >
                  <a :href="websiteItem.href" target="_blank">
                    <div class="item-box">
                      <div class="item-left">
                        <div class="web-logo">{{ websiteItem.name.charAt(0) }}</div>
                      </div>
                      <div class="item-right">
                        <div class="web-title">{{ websiteItem.name }}</div>
                        <div class="web-desc">{{ websiteItem.description }}</div>
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
          <span><ToUpIcon v-if="isShowToUp" :icon-color="'#815353'"/></span>
          <span><ToDownIcon v-if="!isShowToUp" :icon-color="'#815353'"/></span>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="less" scoped>
@media screen and (min-width: 769px) {
  /* 变量 */
  #container {
    --aside-border-right: 4px;
    --box-big-sep-distance: 80px;
    --main-scrollbar-width: 8px;
    --aside-width: 220px;

    --header-height: 60px;
    --search-height: 360px;
    --notice-height: 44px;
    --friend-height: 420px;
    --footer-height: 60px;
    --aside-top-height: 60px;
    --aside-bottom-height: 80px;
    --box-small-sep-distance: 20px;
    --header-text-width: 220px;
    --tool-width: 40px;

    --aside-top-background-color: #a2d1a2;
    --aside-nav-background-color: #815353;
    --aside-bottom-background-color: #d8b174;
    --header-background-color: #f5f5f5;
    --search-background-color: #007dfe;
    --hot-background-color: #931515;
    --notice-background-color: #ffffff;
    --main-background-color: #a4caa4;
    --friend-background-color: #084908;
    --footer-background-color: #ac2626;
    --tool-background-color: #4a5255;

    --transition-time: 0.1s;
  }

  /* 结构 */
  #container {
    position: relative;
    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: row;
    background: #f9f9f9;

    .container-left-box {
      width: var(--aside-width);
      height: 100%;
      display: flex;
      flex-direction: column;
      border-right: var(--aside-border-right) solid #ffffff;
      overflow: hidden;

      .aside-logo-box {
        width: 100%;
        height: 80px;
      }

      .aside-nav-box {
        flex: 1;
        margin: 20px 0;
      }

      .aside-bottom-box {
        width: 100%;
        height: 100px;
      }
    }

    .container-right-box {
      flex: 1;
      display: flex;
      flex-direction: column;

      .content-box {
        width: 100%;
        height: auto;
        margin-top: var(--box-small-sep-distance);
        padding-left: var(--box-big-sep-distance);
        padding-right: calc(var(--box-big-sep-distance) - var(--main-scrollbar-width));
      }

      .footer-box {
        width: 100%;
        height: auto;
        margin-top: var(--box-small-sep-distance);
        padding-left: 0;
        padding-right: 0;
      }
    }

    .container-tool-box {
      position: absolute;
      right: var(--box-small-sep-distance);
      bottom: var(--box-small-sep-distance);
      width: var(--tool-width);
      height: auto;
    }
  }

  /* 滚动条 */
  #container {
    .container-left-box {
      .aside-nav-box {
        overflow-y: scroll;

        // 滚动条整体样式
        &::-webkit-scrollbar {
          width: 6px;
        }

        // 滚动条本身的样式
        &::-webkit-scrollbar-thumb {
          background-color: #ff0000;
          cursor: pointer;
        }
      }
    }

    .container-right-box {
      overflow-y: scroll;
    }
  }

  /* 具体的样式 */
  #container {
    #aside-logo-box {
      width: 100%;
      height: 100%;

      .logo {
        position: relative;
        width: 100%;
        height: 100%;

        img {
          display: block;
          width: 100%;
          height: 100%;
        }

        .logo-desc {
          position: absolute;
          top: 50%;
          left: 100%;
          transform: translateY(-50%);
          width: 300px;
          background: #8d6e63;
          display: none;
        }
      }

      .logo:hover .logo-desc {
        display: block;
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

    #aside-bottom-box {
      width: 100%;
      height: 100px;
      padding: 20px;
    }

    #main-box {
      width: 100%;
      height: auto;
      box-shadow: 0 2px 8px rgb(60 114 139 / 3%);

      .main-section {
        width: 100%;
        height: auto;
        margin-top: 10px;

        .section-title {
          width: 100%;
          height: 30px;
          margin-bottom: 20px;

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

        .section-item {
          display: block;
          width: 100%;
          height: auto;
          margin-bottom: 20px;

          ul {
            width: 100%;
            height: auto;
            display: grid;
            grid-template-columns: 1fr 1fr 1fr 1fr;
            grid-gap: var(--box-small-sep-distance);

            li {
              height: 84px;
              border-radius: 4px;
              box-shadow: 0 2px 8px rgb(60 114 139 / 3%);
              background-color: rgba(255, 255, 255, 0.6);
              border: 2px solid #ffffff;
              transition: all 0.2s ease;
              cursor: pointer;
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
                    width: 100%;
                    height: 100%;
                    background: #3367d6;
                    border-radius: 50%;
                    text-align: center;
                    line-height: 40px;
                    font-size: 20px;
                    font-weight: 500;
                    color: #fff;
                  }
                }

                .item-right {
                  display: flex;
                  flex-direction: column;
                  flex: 1;
                  padding: 10px 0;

                  .web-title {
                    flex: 1;
                    font-size: 14px;
                    font-weight: 500;
                    color: #333333;
                    line-height: 20px;
                    margin-bottom: 2px;
                  }

                  .web-desc {
                    flex: 1;
                    color: #999999;
                    font-size: 12px;
                    line-height: 20px;
                    display: -webkit-box;
                    overflow: hidden;
                    text-overflow: ellipsis;
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

      .main-section:nth-child(n + 2) {
        margin-top: 20px;
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
      width: var(--tool-width);
      height: auto;

      .tool-item-box {
        position: relative;
        width: var(--tool-width);
        height: var(--tool-width);
        background: #ffffff;
        border-radius: 50%;
        cursor: pointer;
        transition: all var(--transition-time);

        span {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          font-size: 1.2em;
          color: #333333;
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

@media screen and (max-width: 768px) {
  /* 变量 */
  #container {
    --aside-border-right: 0px;
    --box-big-sep-distance: 20px;
    --main-scrollbar-width: 0px;
    --aside-width: 0px;

    --header-height: 60px;
    --search-height: 360px;
    --notice-height: 44px;
    --friend-height: 420px;
    --footer-height: 60px;
    --aside-top-height: 60px;
    --aside-bottom-height: 80px;
    --box-small-sep-distance: 20px;
    --header-text-width: 220px;
    --tool-width: 40px;

    --aside-top-background-color: #a2d1a2;
    --aside-nav-background-color: #815353;
    --aside-bottom-background-color: #d8b174;
    --header-background-color: #f5f5f5;
    --search-background-color: #007dfe;
    --hot-background-color: #931515;
    --notice-background-color: #ffffff;
    --main-background-color: #a4caa4;
    --friend-background-color: #084908;
    --footer-background-color: #ac2626;
    --tool-background-color: #4a5255;

    --transition-time: 0.1s;
  }

  /* 结构 */
  #container {
    position: relative;
    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: row;
    background: #f9f9f9;

    .container-left-box {
      width: var(--aside-width);
      height: 100%;
      display: flex;
      flex-direction: column;
      border-right: var(--aside-border-right) solid #ffffff;
      overflow: hidden;

      .aside-logo-box {
        width: 100%;
        height: 80px;
      }

      .aside-nav-box {
        flex: 1;
        margin: 20px 0;
      }

      .aside-bottom-box {
        width: 100%;
        height: 100px;
      }
    }

    .container-right-box {
      flex: 1;
      display: flex;
      flex-direction: column;

      .content-box {
        width: 100%;
        height: auto;
        margin-top: var(--box-small-sep-distance);
        padding-left: var(--box-big-sep-distance);
        padding-right: calc(var(--box-big-sep-distance) - var(--main-scrollbar-width));
      }

      .footer-box {
        width: 100%;
        height: auto;
        margin-top: var(--box-small-sep-distance);
        padding-left: 0;
        padding-right: 0;
      }
    }

    .container-tool-box {
      position: absolute;
      right: var(--box-small-sep-distance);
      bottom: var(--box-small-sep-distance);
      width: var(--tool-width);
      height: auto;
    }
  }

  /* 滚动条 */
  #container {
    .container-left-box {
      .aside-nav-box {
        overflow-y: scroll;

        // 滚动条整体样式
        &::-webkit-scrollbar {
          width: 6px;
        }

        // 滚动条本身的样式
        &::-webkit-scrollbar-thumb {
          background-color: #ff0000;
          cursor: pointer;
        }
      }
    }

    .container-right-box {
      overflow-y: scroll;

      // 滚动条整体样式
      &::-webkit-scrollbar {
        width: 0;
      }

      // 滚动条本身的样式
      &::-webkit-scrollbar-thumb {
        background-color: #ff0000;
        cursor: pointer;
      }
    }
  }

  /* 具体的样式 */
  #container {
    #aside-logo-box {
      width: 100%;
      height: 100%;

      .logo {
        position: relative;
        width: 100%;
        height: 100%;

        img {
          display: block;
          width: 100%;
          height: 100%;
        }

        .logo-desc {
          position: absolute;
          top: 50%;
          left: 100%;
          transform: translateY(-50%);
          width: 300px;
          background: #8d6e63;
          display: none;
        }
      }

      .logo:hover .logo-desc {
        display: block;
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

    #aside-bottom-box {
      width: 100%;
      height: 100px;
      padding: 20px;
    }

    #main-box {
      width: 100%;
      height: auto;
      box-shadow: 0 2px 8px rgb(60 114 139 / 3%);

      .main-section {
        width: 100%;
        height: auto;
        margin-top: 10px;

        .section-title {
          width: 100%;
          height: 30px;
          margin-bottom: 20px;

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

        .section-item {
          display: block;
          width: 100%;
          height: auto;
          margin-bottom: 20px;

          ul {
            width: 100%;
            height: auto;
            display: grid;
            grid-template-columns: 1fr 1fr;
            grid-gap: var(--box-small-sep-distance);

            li {
              height: 84px;
              border-radius: 4px;
              box-shadow: 0 2px 8px rgb(60 114 139 / 3%);
              background-color: rgba(255, 255, 255, 0.6);
              border: 2px solid #ffffff;
              transition: all 0.2s ease;
              cursor: pointer;
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
                    width: 100%;
                    height: 100%;
                    background: #3367d6;
                    border-radius: 50%;
                    text-align: center;
                    line-height: 40px;
                    font-size: 20px;
                    font-weight: 500;
                    color: #fff;
                  }
                }

                .item-right {
                  display: flex;
                  flex-direction: column;
                  flex: 1;
                  padding: 10px 0;

                  .web-title {
                    flex: 1;
                    font-size: 14px;
                    font-weight: 500;
                    color: #333333;
                    line-height: 20px;
                    margin-bottom: 2px;
                  }

                  .web-desc {
                    flex: 1;
                    color: #999999;
                    font-size: 12px;
                    line-height: 20px;
                    display: -webkit-box;
                    overflow: hidden;
                    text-overflow: ellipsis;
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

      .main-section:nth-child(n + 2) {
        margin-top: 20px;
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
      width: var(--tool-width);
      height: auto;

      .tool-item-box {
        position: relative;
        width: var(--tool-width);
        height: var(--tool-width);
        background: #ffffff;
        border-radius: 50%;
        cursor: pointer;
        transition: all var(--transition-time);

        span {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          font-size: 1.2em;
          color: #333333;
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