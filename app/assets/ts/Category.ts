import type WebSite from '@/assets/ts/WebSite.ts'

export default class Category {
  public isSelected: boolean
  public isMouseenter: boolean
  public category: string
  public iconSvg: string
  public children: WebSite[]

  public constructor(params: {
    isSelected: boolean
    isMouseenter: boolean
    category: string
    iconSvg: string
    children: WebSite[]
  }) {
    this.isSelected = params.isSelected
    this.isMouseenter = params.isMouseenter
    this.category = params.category
    this.iconSvg = params.iconSvg
    this.children = params.children
  }
}
