import { createApp, reactive, ref, computed } from "vue"
import { JsonApiClient, $1, $$ } from "@servicestack/client"
import ServiceStackVue from "@servicestack/vue"
import HelloApi from "./components/HelloApi.mjs"
import SrcLink from "./components/SrcLink.mjs"
import ConfirmationPrompt from "./components/ConfirmationPrompt.mjs";
import TaskHistory from "./components/TaskHistory.mjs";
import Notification from "./components/Notification.mjs";
import UserPanel from "./components/UserPanel.mjs";
import GptChat from "./components/GptChat.mjs";
import ChatInput from "./components/ChatInput.mjs";
import ChatApp from "./components/ChatApp.mjs";
import CommandTabs from "./components/CommandTabs.mjs";
import CommandTab from "./components/CommandTab.mjs";
import UpcomingMeetings from "./components/UpcomingMeetings.mjs";

let client = null, Apps = []
let AppData = {
    init:false
}
export { client, Apps }

/** Simple inline component examples */
const Hello = {
    template: `<b>Hello, {{name}}!</b>`,
    props: { name:String }
}
const Counter = {
    template: `<b @click="count++">Counter {{count}}</b>`,
    setup() {
        let count = ref(1)
        return { count }
    }
}
const Plugin = {
    template:`<div>
        <PrimaryButton @click="show=true">Open Modal</PrimaryButton>
        <ModalDialog v-if="show" @done="show=false">
            <div class="p-8">Hello @servicestack/vue!</div>
        </ModalDialog>
    </div>`,
    setup() {
        const show = ref(false)
        return { show }
    }
}

/** Shared Components */
const Components = {
    HelloApi,
    SrcLink,
    Hello,
    Counter,
    Plugin,
    ConfirmationPrompt,
    TaskHistory,
    Notification,
    UserPanel,
    GptChat,
    ChatInput,
    ChatApp,
    CommandTabs,
    CommandTab,
    UpcomingMeetings
}

const alreadyMounted = el => el.__vue_app__ 

/** Mount Vue3 Component
 * @param sel {string|Element} - Element or Selector where component should be mounted
 * @param component 
 * @param [props] {any} */
export function mount(sel, component, props) {
    if (!AppData.init) {
        init(globalThis)
    }
    const el = $1(sel)
    if (alreadyMounted(el)) return
    const app = createApp(component, props)
    app.provide('client', client)
    Object.keys(Components).forEach(name => {
        app.component(name, Components[name])
    })
    app.use(ServiceStackVue)
    app.component('RouterLink', ServiceStackVue.component('RouterLink'))
    app.mount(el)
    Apps.push(app)
    return app
}

export function mountAll() {
    $$('[data-component]').forEach(el => {
        if (alreadyMounted(el)) return
        let componentName = el.getAttribute('data-component')
        if (!componentName) return
        let component = Components[componentName] || ServiceStackVue.component(componentName)
        if (!component) {
            console.error(`Could not create component ${componentName}`)
            return
        }

        let propsStr = el.getAttribute('data-props')
        let props = propsStr && new Function(`return (${propsStr})`)() || {}
        mount(el, component, props)
    })
}

/** @param {any} [exports] */
export function init(exports) {
    if (AppData.init) return
    client = JsonApiClient.create()
    AppData = reactive(AppData)
    AppData.init = true
    mountAll()
    console.log(window.Server)
    if (exports) {
        exports.client = client
        exports.Apps = Apps
    }
}
