import {inject,computed} from "vue"
export default {
    template: `
  <div v-if="isSelected">
    <slot />
  </div>`,
    props: ['title'],
    setup(props) {
        const registerTab = inject('registerTab')
        const isSelectedFn = inject('isSelected');
        registerTab(props)

        const isSelected = computed(() => isSelectedFn(props));
        return {isSelected}
    }
}