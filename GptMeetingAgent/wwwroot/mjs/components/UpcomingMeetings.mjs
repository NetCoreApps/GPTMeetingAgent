import {toRef} from "vue"
export default {
    template: `
<div>
    <h2 class="text-base font-semibold leading-6 text-gray-900">Upcoming meetings</h2>
    <div class="lg:grid lg:grid-cols-12 lg:gap-x-16">
        <ol class="mt-4 divide-y divide-gray-100 text-sm leading-6 lg:col-span-7 xl:col-span-8">
            <li class="relative flex space-x-6 py-2 xl:static" v-for="meeting in meetings">
                <a :href="'/locode/QueryCalendarEvents?Id=' + meeting.id" class="flex-auto ml-2">
                    <img :src="meeting.attendee.profileUrl" alt="" class="h-8 w-8 inline-flex rounded-full" />
                    <h3 class="pr-10 inline-flex px-2 font-semibold text-gray-900 xl:pr-0">{{meeting.attendee.displayName}}</h3>
                    <dl class="mt-2 flex flex-col text-gray-500">
                        <div class="flex items-start space-x-3">
                          <dt class="mt-0.5">
                            <span class="sr-only">Date</span>
                            <svg class="h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                              <path fill-rule="evenodd" d="M5.75 2a.75.75 0 01.75.75V4h7V2.75a.75.75 0 011.5 0V4h.25A2.75 2.75 0 0118 6.75v8.5A2.75 2.75 0 0115.25 18H4.75A2.75 2.75 0 012 15.25v-8.5A2.75 2.75 0 014.75 4H5V2.75A.75.75 0 015.75 2zm-1 5.5c-.69 0-1.25.56-1.25 1.25v6.5c0 .69.56 1.25 1.25 1.25h10.5c.69 0 1.25-.56 1.25-1.25v-6.5c0-.69-.56-1.25-1.25-1.25H4.75z" clip-rule="evenodd" />
                            </svg>
                          </dt>
                          <dd><time datetime="{{meeting.start}}">{{formatDate(meeting.start)}}</time></dd>
                        </div>
                        <div class="flex items-start space-x-3" :title="meeting.subject">
                            <dt class="mt-0.5">
                                <span class="sr-only">Subject</span>
                                <svg class="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                                <path fill="currentColor" d="M13 16H3a1 1 0 0 0 0 2h10a1 1 0 0 0 0-2ZM3 8h18a1 1 0 0 0 0-2H3a1 1 0 0 0 0 2Zm18 3H3a1 1 0 0 0 0 2h18a1 1 0 0 0 0-2Z"/>
                                </svg>
                            </dt>
                            <dd><span>{{meeting.subject}}</span></dd>
                            
                        </div>
                    </dl>
              </a>
            </li>
            <li v-if="!meetings || meetings.length == 0" class="relative flex space-x-6 py-6 xl:static">
                <h3 class="pr-10 font-semibold text-gray-900 xl:pr-0">No Meetings</h3>
            </li>
        </ol>
    </div>
</div>
        
`,
    props:['meetings'],
    setup(props) {
        const meetings = toRef(props,'meetings');
        
        function formatDate(dateStr) {
            let date = new Date(dateStr);
            return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: 'numeric', minute: 'numeric', hour12: true });
        }
        
        return {meetings,formatDate};
    }
}