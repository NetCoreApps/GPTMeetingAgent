import {ref,provide} from "vue"
export default {
    template: `
    <div>
    <div class="flex border-b border-gray-200">
      <button
        v-for="(tab, index) in tabs"
        :key="index"
        @click="selectTab(index)"
        :class="[
          'py-2 px-4 font-semibold text-gray-600',
          { 'border-b-2 border-indigo-600 text-indigo-600': selectedTabIndex === index },
        ]"
      >
        {{ tab.title }}
      </button>
    </div>
    <div class="tab-content mt-4">
      <slot />
    </div>
  </div>
  `,
    setup() {
        const tabs = ref([]);
        const selectedTabIndex = ref(0);

        const registerTab = (tab) => {
            tabs.value.push(tab);
        };

        const selectTab = (index) => {
            selectedTabIndex.value = index;
        };

        provide('registerTab', registerTab);
        provide('isSelected', (tab) => tabs.value[selectedTabIndex.value] === tab);
        
        return { tabs, selectedTabIndex, selectTab,registerTab }
    }
}