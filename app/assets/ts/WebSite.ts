export default class WebSite {
  public name: string
  public href: string
  public desc: string
  public slug: string

  public constructor(params: {
    name: string
    href: string
    desc: string
    slug: string
  }) {
    this.name = params.name
    this.href = params.href
    this.desc = params.desc
    this.slug = params.slug
  }
}
