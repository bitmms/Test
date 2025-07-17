import type WebSite from '@/assets/ts/WebSite.ts'

export class Category {
  public category: string
  public isSelected: boolean
  public isMouseenter: boolean
  public iconSvg: string
  public children: WebSite[]

  public constructor(params: {
    category: string
    isSelected: boolean
    isMouseenter: boolean
    iconSvg: string
    children: WebSite[]
  }) {
    this.category = params.category
    this.isSelected = params.isSelected
    this.isMouseenter = params.isMouseenter
    this.iconSvg = params.iconSvg
    this.children = params.children
  }
}
