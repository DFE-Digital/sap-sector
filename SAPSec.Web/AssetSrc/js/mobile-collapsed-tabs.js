import { Tabs } from '/js/govuk-frontend.min.js'

class MobileCollapsedTabs extends Tabs {
    constructor(t) {
        super(t);
    }
    setupResponsiveChecks() {
        this.mql = window.matchMedia(`(min-width: 0)`), "addEventListener" in this.mql ? this.mql.addEventListener("change", (() => this.checkMode())) : this.mql.addListener((() => this.checkMode())), this.checkMode()
    }
    selectTabById(tabId) {
        if (tabId) {
            const currentTab = this.getCurrentTab();
            const selectedTab = this.getTab(tabId);
            currentTab && selectedTab && (this.hideTab(currentTab), this.showTab(selectedTab), selectedTab.focus())
        }
    }
}

export {
    MobileCollapsedTabs
};