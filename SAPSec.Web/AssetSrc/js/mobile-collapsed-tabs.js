import { Tabs } from '/js/govuk-frontend.min.js'

class MobileCollapsedTabs extends Tabs {
    constructor(t) {
        super(t);

        this.scrollLefts = {};
        this.boundTabPanelScrollEnd = this.onTabPanelScrollEnd.bind(this);
        this.$tabs.forEach((t => {
            const tabPanel = this.getPanel(t);
            tabPanel.addEventListener("scrollend", this.boundTabPanelScrollEnd, !0);
        }))
    }
    teardown() {
        super.teardown();

        this.$tabs.forEach((t => {
            const tabPanel = this.getPanel(t);
            tabPanel.removeEventListener("scrollend", this.boundTabPanelScrollEnd, !0);
        }))
    }
    setupResponsiveChecks() {
        this.mql = window.matchMedia(`(min-width: 0)`), "addEventListener" in this.mql ? this.mql.addEventListener("change", (() => this.checkMode())) : this.mql.addListener((() => this.checkMode())), this.checkMode()
    }
    onTabPanelScrollEnd(e) {
        this.scrollLefts[e.target.id] = e.target.scrollLeft;
    }
    showPanel(t) {
        super.showPanel(t);

        const tabPanel = this.getPanel(t);
        const scrollLeft = this.scrollLefts && this.scrollLefts[tabPanel.id];
        tabPanel && scrollLeft && (tabPanel.scrollLeft = scrollLeft);
    }
    getState() {
        const currentTab = this.getCurrentTab();

        return {
            currentTabId: currentTab.hash,
            scrollLefts: this.scrollLefts
        };
    }
    setState(state) {
        if (state.currentTabId) {
            const currentTab = this.getCurrentTab();
            const selectedTab = this.getTab(state.currentTabId);
            currentTab && selectedTab && (this.hideTab(currentTab), this.showTab(selectedTab))
        }

        if (state.scrollLefts) {
            this.scrollLefts = state.scrollLefts;
            this.$tabs.forEach((t => {
                const tabPanel = this.getPanel(t);
                tabPanel.scrollLeft = this.scrollLefts[tabPanel.id];
            }))
        }
    }
}

export {
    MobileCollapsedTabs
};