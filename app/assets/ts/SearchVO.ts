import type SearchEngine from "~/assets/ts/SearchEngine";

export default class SearchVO {
    public tab: boolean
    public list: SearchEngine[]

    public constructor(params: {
        tab: boolean
        list: SearchEngine[]
    }) {
        this.tab = params.tab
        this.list = params.list
    }
}
