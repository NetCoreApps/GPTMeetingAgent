import {toRef} from "vue";

export default {
    template: `
<div class="">
    <ul role="list" class="divide-y divide-gray-100">
        <li v-for="user in users" class="py-2">
           <div class="flex items-center gap-x-3">
              <img :src="user.profileUrl" alt="" class="h-6 w-6 flex-none rounded-full bg-gray-800">
              <h3 class="px-1 flex-auto truncate text-sm font-semibold leading-6 text-gray-900" :title="user.displayName">{{user.displayName}}</h3>
            </div>
        </li>
    </ul>
</div>`,
    props:['users'],
    setup(props) {
        const users = toRef(props,'users');
        return {users};
    }
}