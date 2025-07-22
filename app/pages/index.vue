<script setup lang="ts">
import type {Category} from "~/assets/ts/Category";
import toolOfToUp from '~/assets/svg/tool/tool-to-up.svg'
import toolOfToDown from '~/assets/svg/tool/tool-to-down.svg'
import data from '~/assets/json/data.json'

// ======= DOM Refs =======
const contextBoxRef: Ref<HTMLElement | null> = ref(null)

// ======= State =======
const isShowToUp: Ref<boolean> = ref(false)
const htmlData: Ref<Category[]> = ref([])

// ======= 批量预加载指定的 SVG 文件 =======
const svgAssets = import.meta.glob('~/assets/svg/{nav,website}/*.svg', {eager: true, import: 'default'})

// 根据传入的路径从预加载的 svgAssets 中获取对应的 SVG 资源路径
const getSvgUrl = (path: string) => {
  if (Object.prototype.hasOwnProperty.call(svgAssets, path)) {
    return svgAssets[path] as string;
  }
  return '~'
}

// 直接在服务器渲染
{
  htmlData.value = data as Category[];
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
            <li v-for="(item, index) in htmlData" :key="index">
              <a :href="'#' + item.category" :title="item.category" :aria-label="item.category">
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
      <div class="content-box">
        <main id="main-box">
          <section v-for="(categoryItem, categoryIndex) in htmlData" :key="categoryIndex" class="main-section">

            <div :id="categoryItem.category" class="section-title">
              <a :href="`/#${categoryItem.category}`" :title="categoryItem.category" :aria-label="categoryItem.category">
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
                          <span v-if="websiteItem.logo === '~'">{{ websiteItem.name.charAt(0) }}</span>
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