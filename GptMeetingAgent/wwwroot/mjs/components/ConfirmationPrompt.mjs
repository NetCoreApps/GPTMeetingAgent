import {ref, toRef} from "vue";

export default {
    template: `
        <ModalDialog @done="cancelAction" v-if="show">
            <div class="relative p-6 bg-white w-full m-auto flex-col flex rounded-lg dark:bg-black dark:text-white">
                    <h2 class="text-xl font-semibold mb-4">{{ title }}</h2>
                    <p class="text-gray-700 mb-6">{{ message }}</p>
                    <div class="flex justify-end space-x-4">
                        <button class="bg-gray-300 text-gray-900 px-4 py-2 rounded-md focus:outline-none"
                                @click="cancelAction">
                                {{ cancelText }}
                        </button>
                        <button class="bg-red-500 text-white px-4 py-2 rounded-md focus:outline-none"
                            @click="confirmAction">
                            {{ confirmText }}
                        </button>
                    </div>
                </div>
        </ModalDialog>
`,
    props: ['show','title', 'message', 'cancelText', 'confirmText'],
    setup(props) {
        const title = toRef(props, 'title');
        const message = toRef(props, 'message');
        const cancelText = ref(props.cancelText);
        const confirmText = ref(props.confirmText);

        return {title, message, cancelText, confirmText};
    },
    methods: {
        cancelAction() {
            this.$emit('cancel');
        },
        confirmAction() {
            this.$emit('confirm');
        },
    },
};

