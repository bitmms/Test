export default class WebSite {
  public name: string
  public description: string
  public href: string

  public constructor(params: {
    name: string
    description: string
    href: string
  }) {
    this.name = params.name
    this.description = params.description
    this.href = params.href
  }
}
