export default class WebSite {
  public name: string
  public href: string
  public desc: string
  public slug: string
  public logo: string

  public constructor(params: {
    name: string
    href: string
    desc: string
    slug: string
    logo: string
  }) {
    this.name = params.name
    this.href = params.href
    this.desc = params.desc
    this.slug = params.slug
    this.logo = params.logo
  }
}
